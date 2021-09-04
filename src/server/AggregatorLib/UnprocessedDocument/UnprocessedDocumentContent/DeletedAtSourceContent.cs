namespace AggregatorLib
{
    public class DeletedAtSourceContent : UnprocessedDocumentContent
    {
        public DeletedAtSourceContent() { }

        public override string ContentType => "DeletedAtSource";
    }
}
