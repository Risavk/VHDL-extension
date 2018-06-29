using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace VHDL_Extension.Errors
{
    class VhdlErrorsSnapshot : WpfTableEntriesSnapshotBase
    {
        private readonly string _filePath;
        private readonly int _versionNumber;

        // We're not using an immutable list here but we cannot modify the list in any way once we've published the snapshot.
        public readonly List<VhdlError> Errors = new List<VhdlError>();

        public VhdlErrorsSnapshot NextSnapshot;

        internal VhdlErrorsSnapshot(string filePath, int versionNumber)
        {
            _filePath = filePath;
            _versionNumber = versionNumber;
        }

        public override int Count => Errors.Count;

        public override int VersionNumber => _versionNumber;

        public override int IndexOf(int currentIndex, ITableEntriesSnapshot newerSnapshot)
        {
            // This and TranslateTo() are used to map errors from one snapshot to a different one (that way the error list can do things like maintain the selection on an error
            // even when the snapshot containing the error is replaced by a new one).
            //
            // You only need to implement Identity() or TranslateTo() and, of the two, TranslateTo() is more efficient for the error list to use.

            // Map currentIndex to the corresponding index in newerSnapshot (and keep doing it until either
            // we run out of snapshots, we reach newerSnapshot, or the index can no longer be mapped forward).
            var currentSnapshot = this;
            do
            {
                Debug.Assert(currentIndex >= 0);
                Debug.Assert(currentIndex < currentSnapshot.Count);

                currentIndex = currentSnapshot.Errors[currentIndex].NextIndex;

                currentSnapshot = currentSnapshot.NextSnapshot;
            }
            while ((currentSnapshot != null) && (currentSnapshot != newerSnapshot) && (currentIndex >= 0));

            return currentIndex;
        }

        public override bool TryGetValue(int index, string columnName, out object content)
        {
            if ((index >= 0) && (index < this.Errors.Count))
            {
                if (columnName == StandardTableKeyNames.DocumentName)
                {
                    // We return the full file path here. The UI handles displaying only the Path.GetFileName().
                    content = _filePath;
                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorCategory)
                {
                    content = "Documentation";
                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorSource)
                {
                    content = "VHDL";
                    return true;
                }
                else if (columnName == StandardTableKeyNames.Line)
                {
                    // Line and column numbers are 0-based (the UI that displays the line/column number will add one to the value returned here).
                    content = this.Errors[index].Span.Start.GetContainingLine().LineNumber;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.Column)
                {
                    var position = this.Errors[index].Span.Start;
                    var line = position.GetContainingLine();
                    content = position.Position - line.Start.Position;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.Text) //TODO setup proper error text
                {
                    //content = string.Format(CultureInfo.InvariantCulture, "{0}", this.Errors[index].Span.GetText());

                    switch (Errors[index].ErrorType)
                    {
                        case VhdlErrorType.ClosingSemiColon:
                            content = "; expected";
                            break;
                        case VhdlErrorType.PortEnd:
                            content = "End of port expected (\");\")";
                            break;
                        case VhdlErrorType.Other:
                        default:
                            content = "Unspecified type of error";
                            break;
                    }

                    return true;
                }
                else if (columnName == StandardTableKeyNames2.TextInlines)
                {
                    var inlines = new List<Inline>();

                    inlines.Add(new Run(this.Errors[index].Span.GetText())
                    {
                        FontWeight = FontWeights.ExtraBold
                    });

                    content = inlines;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorSeverity)
                {
                    content = __VSERRORCATEGORY.EC_ERROR;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorSource)
                {
                    content = ErrorSource.Other;

                    return true;
                }
                else if (columnName == StandardTableKeyNames.BuildTool)
                {
                    content = "vhdlChecker";

                    return true;
                }
                else if (columnName == StandardTableKeyNames.ErrorCode)
                {
                    switch (Errors[index].ErrorType)
                    {
                        case VhdlErrorType.ClosingSemiColon:
                            content = 1;
                            break;
                        case VhdlErrorType.PortEnd:
                            content = 2;
                            break;
                        case VhdlErrorType.Other:
                            content = 99;
                            break;
                        default:
                            content = -1;
                            break;
                    }

                    return true;
                }
                else if ((columnName == StandardTableKeyNames.ErrorCodeToolTip) || (columnName == StandardTableKeyNames.HelpLink))
                {
                    object obj;
                    if (TryGetValue(index, StandardTableKeyNames.Text, out obj))
                    {
                        var text = obj as string;
                        content = string.Format(CultureInfo.InvariantCulture, "http://www.google.com/search?q={0} {1}", "VHDL", text);
                    }
                    else
                    {
                        content = "";
                    }

                    return true;
                }

                //TODO see below
                // We should also be providing values for StandardTableKeyNames.Project & StandardTableKeyNames.ProjectName but that is
                // beyond the scope of this sample.
            }

            content = null;
            return false;
        }

        //TODO maybe implement details content
        public override bool CanCreateDetailsContent(int index)
        {
            return false;
        }

        public override bool TryCreateDetailsStringContent(int index, out string content)
        {
            content = null;

            return (content != null);
        }
    }
}
