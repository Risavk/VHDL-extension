using System.ComponentModel.Composition;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace VHDL_Extension
{
    /// <summary>
    /// Classification type definition export for VHDL_classifier
    /// </summary>
    internal static class VHDL_ClassificationDefinitions
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        #region Content type and File Extension Definition

        [Export]
        [Name("VHDL")]
        [BaseDefinition("text")]
        internal static ContentTypeDefinition VhdlContentTypeDefinition;

        [Export]
        [FileExtension(".vhd")]
        [ContentType("VHDL")]
        internal static FileExtensionToContentTypeDefinition VhdlFileExtensionDefinition;

        #endregion

        #region Classification Type Definition

        [Export]
        [Name("VHDL")]
        internal static ClassificationTypeDefinition vhdlClassificationTypeDefinition;

        [Export]
        [Name("VHDL.reserved")]
        [BaseDefinition("VHDL")]
        internal static ClassificationTypeDefinition VhdlReservedDefinition;

        [Export]
        [Name("VHDL.customkeyword")]
        [BaseDefinition("VHDL")]
        internal static ClassificationTypeDefinition VhdlCustomKeywordDefinition;

        [Export]
        [Name("VHDL.comment")]
        [BaseDefinition("VHDL")]
        internal static ClassificationTypeDefinition vhdlCommentDefinition;

        [Export]
        [Name("VHDL.ieee")]
        [BaseDefinition("VHDL")]
        internal static ClassificationTypeDefinition VhdlieeeDefinition;

        [Export]
        [Name("VHDL.number")]
        [BaseDefinition("VHDL")]
        internal static ClassificationTypeDefinition VhdlNumberDefinition;

        [Export]
        [Name("VHDL.string")]
        [BaseDefinition("VHDL")]
        internal static ClassificationTypeDefinition VhdlStringDefinition;

        #endregion

#pragma warning restore 169
    }
}
