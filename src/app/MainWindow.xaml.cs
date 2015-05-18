using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SO6
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        async void UpdateRTB()
        {
            rtb.IsEnabled = false;
            var doc = rtb.Document;
            foreach (var par in GetParagraphs(doc.Blocks).ToList())
                await UpdateParagraph(par);
            rtb.IsEnabled = true;
        }

        IEnumerable<Paragraph> GetParagraphs(BlockCollection blockCollection)
        {
            foreach (var block in blockCollection)
            {
                var para = block as Paragraph;
                if (para != null)
                {
                    yield return para;
                }
                else
                {
                    foreach (var innerPara in GetParagraphs(block.SiblingBlocks))
                        yield return innerPara;
                }
            }
        }

        async Task UpdateParagraph(Paragraph par)
        {
            var completeTextRange = new TextRange(par.ContentStart, par.ContentEnd);
            completeTextRange.ClearAllProperties();

            var texts = ExtractText(par.Inlines);
            var positionsAndBrushes =
                (from qualifiedToken in await Lexer.Parse(texts)
                 let brush = GetBrushForTokenType(qualifiedToken.Type)
                 where brush != null
                 let start = qualifiedToken.StartPosition.GetPositionAtOffset(qualifiedToken.StartOffset)
                 let end = qualifiedToken.EndPosition.GetPositionAtOffset(qualifiedToken.EndOffset)
                 let position = new TextRange(start, end)
                 select new { position, brush }).ToList();

            foreach (var pb in positionsAndBrushes)
                pb.position.ApplyPropertyValue(TextElement.ForegroundProperty, pb.brush);
        }

        Brush GetBrushForTokenType(TokenType tokenType)
        {
            switch (tokenType)
            {
            case TokenType.Comment: return Brushes.LightGray;
            case TokenType.Keyword: return Brushes.OrangeRed;
            case TokenType.Number: return Brushes.Cyan;
            case TokenType.Punct: return Brushes.Gray;
            case TokenType.String: return Brushes.DarkRed;
            }
            return null;
        }

        IEnumerable<RawText> ExtractText(IEnumerable<Inline> inlines)
        {
            return inlines.SelectMany(ExtractText);
        }

        IEnumerable<RawText> ExtractText(Inline inline)
        {
            return ExtractTextImpl((dynamic)inline);
        }

        IEnumerable<RawText> ExtractTextImpl(Run run)
        {
            return new[] { new RawText() { Text = run.Text, Start = run.ContentStart } };
        }

        IEnumerable<RawText> ExtractTextImpl(LineBreak br)
        {
            return new[] { new RawText() { Text = "\n", Start = br.ContentStart } };
        }

        IEnumerable<RawText> ExtractTextImpl(Span span)
        {
            return ExtractText(span.Inlines);
        }

        IEnumerable<RawText> ExtractTextImpl(Inline inline)
        {
            return Enumerable.Empty<RawText>();
        }

        void ForceFormatting(object sender, RoutedEventArgs e)
        {
            UpdateRTB();
        }
    }
}
