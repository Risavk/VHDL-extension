using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace VHDL_Extension
{
    /// <summary>
    /// Classifier that classifies all text as an instance of the "VHDL_classifier" classification type.
    /// </summary>
    internal class VHDL_classifier : IClassifier
    {
        /// <summary>
        /// Classification type.
        /// </summary>
        private IClassificationTypeRegistryService _classificationTypeRegistry;

        private List<string> keywords;

        /// <summary>
        /// Initializes a new instance of the <see cref="VHDL_classifier"/> class.
        /// </summary>
        /// <param name="registry">Classification registry.</param>
        internal VHDL_classifier(IClassificationTypeRegistryService registry)
        {
            this._classificationTypeRegistry = registry;

            //Load all keywords
            try
            {
                var path = Path.GetDirectoryName(typeof(VHDL_classifier).Assembly.Location);
                if (path == null)
                {
                    MessageBox.Show("Couldn't load keywords");
                    return;
                }
                path = Path.Combine(path, "keywords.csv");
                using (var reader = new StreamReader(path))
                {
                    keywords = new List<string>();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (line != null)
                        {
                            var values = line.Split(';');

                            keywords.Add(values[0]);
                        }
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error loading keywords");
            }
        }

        #region IClassifier

#pragma warning disable 67

        /// <summary>
        /// An event that occurs when the classification of a span of text has changed.
        /// </summary>
        /// <remarks>
        /// This event gets raised if a non-text change would affect the classification in some way,
        /// for example typing /* would cause the classification to change in C# without directly
        /// affecting the span.
        /// </remarks>
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67

        /// <summary>
        /// Gets all the <see cref="ClassificationSpan"/> objects that intersect with the given range of text.
        /// </summary>
        /// <remarks>
        /// This method scans the given SnapshotSpan for potential matches for this classification.
        /// In this instance, it classifies everything and returns each span as a new ClassificationSpan.
        /// </remarks>
        /// <param name="span">The span currently being classified.</param>
        /// <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            ITextSnapshot snapshot = span.Snapshot;
            List<ClassificationSpan> spans = new List<ClassificationSpan>();

            if (snapshot.Length == 0)
            {
                return spans;
            }

            int startno = span.Start.GetContainingLine().LineNumber;
            int endno = (span.End - 1).GetContainingLine().LineNumber;

            for (int i = startno; i <= endno; i++)
            {
                ITextSnapshotLine line = snapshot.GetLineFromLineNumber(i);

                IClassificationType type = null;
                string text = line.Snapshot.GetText(new SnapshotSpan(line.Start, line.Length));
                var values = text.Split(' ', '(');

                if (text.StartsWith("--", StringComparison.Ordinal)) //Text is a comment
                {
                    type = _classificationTypeRegistry.GetClassificationType("VHDL.comment");
                }
                else //Text is not a comment, so see if it is a keyword
                {
                    int startposition = line.Start.Position;
                    foreach (var word in values)
                    {
                        if (word.Trim().StartsWith("--", StringComparison.Ordinal))
                        {
                            type = _classificationTypeRegistry.GetClassificationType("VHDL.comment");
                            spans.Add(CreateClassificationSpan(line, startposition, line.End, type));
                            break;
                        }
                        else if (keywords.Contains(word.Trim().ToLower()))
                        {
                            type = _classificationTypeRegistry.GetClassificationType("VHDL.reserved");
                            spans.Add(CreateClassificationSpan(line, startposition, startposition + word.Length, type));
                        }
                        else if (word.Trim().ToLower().StartsWith("ieee"))
                        {
                            spans.Add(CreateClassificationSpan(line, startposition, line.End.Position, _classificationTypeRegistry.GetClassificationType("VHDL.ieee")));
                        }

                        startposition += word.Length + 1; //Account for the whitespace
                    }

                    type = null;
                }

                if (type != null)
                {
                    spans.Add(new ClassificationSpan(line.Extent, type));
                }
            }

            return spans;
        }

        private ClassificationSpan CreateClassificationSpan(ITextSnapshotLine line, int startpos, int endpos, IClassificationType type)
        {
            return new ClassificationSpan(new SnapshotSpan(new SnapshotPoint(line.Snapshot, startpos), new SnapshotPoint(line.Snapshot, endpos)), type);
        }

        #endregion
    }
}
