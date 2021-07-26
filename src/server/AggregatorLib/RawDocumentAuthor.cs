namespace AggregatorLib
{
    public class RawDocumentAuthor
    {
        public string Name { get; protected set; } 
        public string Context { get; protected set; }  // e.g. Wordpress blogger, youtube channel, reddit username, etc.

        public RawDocumentAuthor(string name, string context)
        {
            Name = name;
            Context = context;
        }
    }
}
