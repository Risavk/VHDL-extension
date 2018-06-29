using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace VHDL_Extension.Errors
{
    class VhdlCheckerTagger : ITagger<IErrorTag>, IDisposable
    {
        private readonly VhdlChecker _vhdlChecker;
        private VhdlErrorsSnapshot _vhdlErrors;

        internal VhdlCheckerTagger(VhdlChecker vhdlChecker)
        {
            _vhdlChecker = vhdlChecker;
            _vhdlErrors = vhdlChecker.LastSpellingErrors;

            vhdlChecker.AddTagger(this);
        }

        internal void UpdateErrors(ITextSnapshot currentSnapshot, VhdlErrorsSnapshot vhdlErrors)
        {
            var oldVhdlErrors = _vhdlErrors;
            _vhdlErrors = vhdlErrors;

            var h = this.TagsChanged;
            if (h != null)
            {
                // Raise a single tags changed event over the span that could have been affected by the change in the errors.
                int start = int.MaxValue;
                int end = int.MinValue;

                if ((oldVhdlErrors != null) && (oldVhdlErrors.Errors.Count > 0))
                {
                    start = oldVhdlErrors.Errors[0].Span.Start.TranslateTo(currentSnapshot, PointTrackingMode.Negative);
                    end = oldVhdlErrors.Errors[oldVhdlErrors.Errors.Count - 1].Span.End.TranslateTo(currentSnapshot, PointTrackingMode.Positive);
                }

                if (vhdlErrors.Count > 0)
                {
                    start = Math.Min(start, vhdlErrors.Errors[0].Span.Start.Position);
                    end = Math.Max(end, vhdlErrors.Errors[vhdlErrors.Errors.Count - 1].Span.End.Position);
                }

                if (start < end)
                {
                    h(this, new SnapshotSpanEventArgs(new SnapshotSpan(currentSnapshot, Span.FromBounds(start, end))));
                }
            }
        }

        public void Dispose()
        {
            // Called when the tagger is no longer needed (generally when the ITextView is closed).
            _vhdlChecker.RemoveTagger(this);
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (_vhdlErrors != null)
            {
                foreach (var error in _vhdlErrors.Errors)
                {
                    if (spans.IntersectsWith(error.Span))
                    {
                        yield return new TagSpan<IErrorTag>(error.Span, new ErrorTag(PredefinedErrorTypeNames.Warning));
                    }
                }
            }
        }
    }
}
