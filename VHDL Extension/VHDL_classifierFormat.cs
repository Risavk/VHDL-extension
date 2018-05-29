using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace VHDL_Extension
{
    #region Classification Format Productions

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "VHDL.reserved")]
    [Name("VHDL.reserved")]
    internal sealed class VHDLReservedWord : ClassificationFormatDefinition
    {
        public VHDLReservedWord()
        {
            ForegroundColor = Colors.Blue;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "VHDL.comment")]
    [Name("VHDL.comment")]
    internal sealed class VHDLCommentFormat : ClassificationFormatDefinition
    {
        public VHDLCommentFormat()
        {
            ForegroundColor = Colors.Green;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "VHDL.ieee")]
    [Name("VHDL.ieee")]
    internal sealed class VHDLieeeFormat : ClassificationFormatDefinition
    {
        public VHDLieeeFormat()
        {
            ForegroundColor = Colors.DeepPink;
        }
    }

    #endregion
}
