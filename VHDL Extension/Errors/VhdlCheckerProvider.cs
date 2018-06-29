using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace VHDL_Extension.Errors
{
    /// <summary>
    /// Factory for the <see cref="ITagger{T}"/>. There will be one instance of this class/VS session.
    /// 
    /// It is also the <see cref="ITableDataSource"/> that reports spelling errors in comments.
    /// </summary>
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("vhdl")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    internal sealed class VhdlCheckerProvider : IViewTaggerProvider, ITableDataSource
    {
        internal readonly ITableManager ErrorTableManager;
        internal readonly ITextDocumentFactoryService TextDocumentFactoryService;
        internal readonly IClassifierAggregatorService ClassifierAggregatorService;

        const string VhdlCheckerDataSource = "VhdlChecker";

        private readonly List<VhdlSinkManager> _managers = new List<VhdlSinkManager>();      // Also used for locks
        private readonly List<VhdlChecker> _spellCheckers = new List<VhdlChecker>();

        [ImportingConstructor]
        internal VhdlCheckerProvider([Import]ITableManagerProvider provider, [Import] ITextDocumentFactoryService textDocumentFactoryService, [Import] IClassifierAggregatorService classifierAggregatorService)
        {
            ErrorTableManager = provider.GetTableManager(StandardTables.ErrorsTable);
            TextDocumentFactoryService = textDocumentFactoryService;

            ClassifierAggregatorService = classifierAggregatorService;

            ErrorTableManager.AddSource(this, StandardTableColumnDefinitions.DetailsExpander,
                StandardTableColumnDefinitions.ErrorSeverity, StandardTableColumnDefinitions.ErrorCode,
                StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.BuildTool,
                StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.ErrorCategory,
                StandardTableColumnDefinitions.Text, StandardTableColumnDefinitions.DocumentName, StandardTableColumnDefinitions.Line, StandardTableColumnDefinitions.Column);
        }

        /// <summary>
        /// Create a tagger that does spell checking on the view/buffer combination.
        /// </summary>
        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            ITagger<T> tagger = null;

            // Only attempt to spell check on the view's edit buffer (and multiple views could have that buffer open simultaneously so
            // only create one instance of the spell checker.
            if ((buffer == textView.TextBuffer) && (typeof(T) == typeof(IErrorTag)))
            {
                var spellChecker = buffer.Properties.GetOrCreateSingletonProperty(typeof(VhdlChecker), () => new VhdlChecker(this, textView, buffer));

                // This is a thin wrapper around the SpellChecker that can be disposed of without shutting down the SpellChecker
                // (unless it was the last tagger on the spell checker).
                tagger = new VhdlCheckerTagger(spellChecker) as ITagger<T>;
            }

            return tagger;
        }

        #region ITableDataSource members
        public string DisplayName
        {
            get
            {
                // This string should, in general, be localized since it is what would be displayed in any UI that lets the end user pick
                // which ITableDataSources should be subscribed to by an instance of the table control. It really isn't needed for the error
                // list however because it autosubscribes to all the ITableDataSources.
                return "VHDL Checker";
            }
        }

        public string Identifier
        {
            get
            {
                return VhdlCheckerDataSource;
            }
        }

        public string SourceTypeIdentifier
        {
            get
            {
                return StandardTableDataSources.ErrorTableDataSource;
            }
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            // This method is called to each consumer interested in errors. In general, there will be only a single consumer (the error list tool window)
            // but it is always possible for 3rd parties to write code that will want to subscribe.
            return new VhdlSinkManager(this, sink);
        }
        #endregion

        public void AddSinkManager(VhdlSinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (_managers)
            {
                _managers.Add(manager);

                // Add the pre-existing spell checkers to the manager.
                foreach (var spellChecker in _spellCheckers)
                {
                    manager.AddSpellChecker(spellChecker);
                }
            }
        }

        public void RemoveSinkManager(VhdlSinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (_managers)
            {
                _managers.Remove(manager);
            }
        }

        public void AddSpellChecker(VhdlChecker vhdlChecker)
        {
            // This call will always happen on the UI thread (it is a side-effect of adding or removing the 1st/last tagger).
            lock (_managers)
            {
                _spellCheckers.Add(vhdlChecker);

                // Tell the preexisting managers about the new spell checker
                foreach (var manager in _managers)
                {
                    manager.AddSpellChecker(vhdlChecker);
                }
            }
        }

        public void RemoveSpellChecker(VhdlChecker vhdlChecker)
        {
            // This call will always happen on the UI thread (it is a side-effect of adding or removing the 1st/last tagger).
            lock (_managers)
            {
                _spellCheckers.Remove(vhdlChecker);

                foreach (var manager in _managers)
                {
                    manager.RemoveSpellChecker(vhdlChecker);
                }
            }
        }

        public void UpdateAllSinks()
        {
            lock (_managers)
            {
                foreach (var manager in _managers)
                {
                    manager.UpdateSink();
                }
            }
        }
    }
}
