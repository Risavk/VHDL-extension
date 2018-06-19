using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace VHDL_Extension.Completion
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("vhdl")]
    [Name("vhdlCompletion")]
    class VhdlCompletionSourceProvider : ICompletionSourceProvider
    {
        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new VhdlCompletionSource(textBuffer);
        }
    }

    class VhdlCompletionSource : ICompletionSource
    {
        private ITextBuffer _buffer;
        private bool _disposed = false;

        public VhdlCompletionSource(ITextBuffer buffer)
        {
            _buffer = buffer;
        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {
            if (_disposed)
                throw new ObjectDisposedException("VhdlCompletionSource");

            List<Microsoft.VisualStudio.Language.Intellisense.Completion> completions = new List<Microsoft.VisualStudio.Language.Intellisense.Completion>();

            foreach (var keyword in Helper.Keywords)
            {
                completions.Add(new Microsoft.VisualStudio.Language.Intellisense.Completion(keyword));
            }

            foreach (var signal in VhdlStructerMapper.VhdlEntity.Port.Signals)
            {
                completions.Add(new Microsoft.VisualStudio.Language.Intellisense.Completion(signal.Name, signal.Name, signal.Type, null, "test"));
            }

            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            var triggerPoint = (SnapshotPoint)session.GetTriggerPoint(snapshot);

            if (triggerPoint == null)
                return;

            var line = triggerPoint.GetContainingLine();
            SnapshotPoint start = triggerPoint;

            while (start > line.Start && !char.IsWhiteSpace((start - 1).GetChar()))
            {
                start -= 1;
            }

            var applicableTo = snapshot.CreateTrackingSpan(new SnapshotSpan(start, triggerPoint), SpanTrackingMode.EdgeInclusive);

            completionSets.Add(new CompletionSet("All", "All", applicableTo, completions, Enumerable.Empty<Microsoft.VisualStudio.Language.Intellisense.Completion>()));
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}
