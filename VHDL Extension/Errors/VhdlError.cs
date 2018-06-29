using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;

namespace VHDL_Extension.Errors
{
    public enum VhdlErrorType
    {
        ClosingSemiColon,
        PortEnd,
        Other
    }

    class VhdlError
    {
        public VhdlErrorType ErrorType;
        public string Text { get; set; }
        public int Start { get; set; }
        public int Stop { get; set; }
        public ITextSnapshotLine Line { get; set; }
        public readonly SnapshotSpan Span;

        // This is used by SpellingErrorsSnapshot.TranslateTo() to map this error to the corresponding error in the next snapshot.
        public int NextIndex = -1;

        public VhdlError(SnapshotSpan span, VhdlErrorType errorType)
        {
            this.Span = span;
            this.ErrorType = errorType;
            //this.Line = line;
        }

        public static VhdlError Clone(VhdlError error)
        {
            return new VhdlError(error.Span, error.ErrorType);
        }

        public static VhdlError CloneAndTranslateTo(VhdlError error, ITextSnapshot newSnapshot)
        {
            var newSpan = error.Span.TranslateTo(newSnapshot, SpanTrackingMode.EdgeExclusive);

            // We want to only translate the error if the length of the error span did not change (if it did change, it would imply that
            // there was some text edit inside the error and, therefore, that the error is no longer valid).
            return (newSpan.Length == error.Span.Length)
                ? new VhdlError(newSpan, error.ErrorType)
                : null;
        }
    }
}
