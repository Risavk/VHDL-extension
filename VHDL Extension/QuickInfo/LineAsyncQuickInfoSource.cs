using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Adornments;
using VHDL_Extension.Types;

namespace VHDL_Extension.QuickInfo
{
    internal sealed class LineAsyncQuickInfoSource : IAsyncQuickInfoSource
    {
        //private static readonly ImageId _icon = KnownMonikers.AbstractCube.ToImageId();

        private ITextBuffer _textBuffer;

        public LineAsyncQuickInfoSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        // This is called on a background thread.
        public Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var triggerPoint = session.GetTriggerPoint(_textBuffer.CurrentSnapshot);

            if (triggerPoint != null)
            {
                var point = (SnapshotPoint)triggerPoint;
                var line = point.GetContainingLine();
                var lineNumber = point.GetContainingLine().LineNumber;
                var lineSpan = _textBuffer.CurrentSnapshot.CreateTrackingSpan(line.Extent, SpanTrackingMode.EdgeInclusive);

                var snapshotspan = new SnapshotSpan(point, point);

                VHDL_classifier classifier;
                if (session.TextView.TextBuffer.Properties.TryGetProperty(typeof(VHDL_classifier), out classifier))
                {
                    var tags = classifier.GetClassificationSpans(snapshotspan);
                    foreach (var classificationSpan in tags)
                    {
                        if (classificationSpan.Span.Contains(point))
                        {
                            var type = classificationSpan.ClassificationType;
                            var text = classificationSpan.Span.GetText();

                            ISignal portType = VhdlStructerMapper.VhdlEntity.Port.Signals.FirstOrDefault(p => p.Name == text.Trim());
                            if (portType == null)
                            {
                                portType = VhdlStructerMapper.VhdlEntity.Architecture.Signals.FirstOrDefault(p => p.Name == text.Trim());
                            }

                            if (portType != null)
                            {
                                var typeElm = new ContainerElement(
                                    ContainerElementStyle.Wrapped,
                                    new ClassifiedTextElement(
                                        new ClassifiedTextRun("VHDL.reservedword", portType.Type),
                                        new ClassifiedTextRun("none", " "),
                                        new ClassifiedTextRun(type.Classification, text.Trim())
                                    ));

                                return Task.FromResult(new QuickInfoItem(lineSpan, typeElm));
                            }
                        }
                    }
                }

                /*
                var lineNumberElm = new ContainerElement(
                    ContainerElementStyle.Wrapped,
                    //new ImageElement(_icon),
                    new ClassifiedTextElement(
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "Line number: "),
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.Identifier, $"{lineNumber + 1}")
                    ));

                var dateElm = new ContainerElement(
                    ContainerElementStyle.Stacked,
                    lineNumberElm,
                    new ClassifiedTextElement(
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.SymbolDefinition, "The current date: "),
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, DateTime.Now.ToShortDateString())
                    ));
                    */
            }

            return Task.FromResult<QuickInfoItem>(null);
        }

        public void Dispose()
        {
            // This provider does not perform any cleanup.
        }
    }
}
