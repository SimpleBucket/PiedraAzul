using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace PiedraAzul.ApplicationServices.AutoCompleteServices
{
    public abstract class BaseLuceneService : IDisposable
    {
        protected readonly Analyzer Analyzer;
        protected readonly IndexWriter Writer;
        protected readonly SearcherManager SearcherManager;

        protected static readonly LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;

        protected BaseLuceneService(string indexPath)
        {
            if (!System.IO.Directory.Exists(indexPath))
                System.IO.Directory.CreateDirectory(indexPath);

            var dir = FSDirectory.Open(indexPath);

            if (IndexWriter.IsLocked(dir))
                IndexWriter.Unlock(dir);

            Analyzer = new StandardAnalyzer(AppLuceneVersion);

            var config = new IndexWriterConfig(AppLuceneVersion, Analyzer);

            Writer = new IndexWriter(dir, config);

            SearcherManager = new SearcherManager(Writer, true, null);
        }

        protected string BuildPrefixQuery(string text)
        {
            var escaped = QueryParserBase.Escape(text);
            var terms = escaped.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (terms.Length == 0)
                return "*";

            return string.Join(" AND ", terms.Select(t => t + "*"));
        }

        public void Dispose()
        {
            SearcherManager?.Dispose();
            Writer?.Dispose();
            Analyzer?.Dispose();
        }
    }
}
