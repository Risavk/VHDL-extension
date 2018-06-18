using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace VHDL_Extension
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("vhdl")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class TestViewCreationListener : IWpfTextViewCreationListener
    {
        [Import] internal IEditorFormatMapService FormatMapService = null;

        public void TextViewCreated(IWpfTextView textView)

        {

            new Listener(textView);

        }

    }

    class Listener
    {
        ITextView _textView;
        ITextDataModel _dataModel;

        public Listener(ITextView textView)
        {

            _textView = textView;

            _dataModel = _textView.TextViewModel.DataModel;

            _textView.Closed += this.OnClosed;

            _dataModel.ContentTypeChanged += this.OnContentTypeChanged;

            //TODO consider using textView.TextBuffer.ChangedHighPriority (which means it will be processed before the view's layout

            //and, as a result, the OnBufferChanged handler below won't force a second layout ... it will just make the normal layout

            //format more text).

            textView.TextBuffer.PostChanged += this.OnBufferChanged;

        }

        void OnContentTypeChanged(object sender, TextDataModelContentTypeChangedEventArgs e)
        {
            if (!e.AfterContentType.IsOfType("vhdl"))

            {

                //We are no longer a "code" content type so unhook all the events so we can be garbage collected.

                this.OnClosed(null, null);

            }

        }

        private void OnBufferChanged(object sender, EventArgs e)

        {

            //Force the entire view to reformat.

            //TODO only do this if a line containing tabs was modified.

            int oldTabSize = _textView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);

            _textView.Options.SetOptionValue(DefaultOptions.TabSizeOptionId, oldTabSize + 1);

            _textView.Options.SetOptionValue(DefaultOptions.TabSizeOptionId, oldTabSize);

        }

        private void OnClosed(object sender, EventArgs e)

        {

            _textView.Closed -= this.OnClosed;

            _textView.TextBuffer.PostChanged -= this.OnBufferChanged;

            _dataModel.ContentTypeChanged -= this.OnContentTypeChanged;

        }
    }
}
