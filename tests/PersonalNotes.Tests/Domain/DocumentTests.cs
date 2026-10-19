using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Tests.Domain
{
    public class DocumentTests
    {
        private static readonly DateTime Now = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        private static readonly string FolderId = ObjectIds.New();

        [Fact]
        public void Create_SetsUpdatedOnEqualToCreatedOn()
        {
            var document = Document.Create(FolderId, " Diseño de la API ", "<p>JWT</p>", Priority.High, Now);

            Assert.Equal(("Diseño de la API", "<p>JWT</p>", Priority.High),
                (document.Title, document.Content, document.Priority));
            Assert.Equal((Now, Now), (document.CreatedOn, document.UpdatedOn));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_EmptyTitle_IsPublishedAsUntitled(string? title)
        {
            var document = Document.Create(FolderId, title, null, Priority.None, Now);

            Assert.Equal(Document.DefaultTitle, document.Title);
            Assert.Equal(string.Empty, document.Content);
        }

        [Fact]
        public void Create_RejectsTooLongTitle_AndInvalidFolder_AndUnknownPriority()
        {
            Assert.Throws<DomainException>(() =>
                Document.Create(FolderId, new string('x', Document.MaxTitleLength + 1), "", Priority.None, Now));
            Assert.Throws<DomainException>(() => Document.Create("carpeta", "Nota", "", Priority.None, Now));
            Assert.Throws<DomainException>(() => Document.Create(FolderId, "Nota", "", (Priority)42, Now));
        }

        [Fact]
        public void Create_RejectsContentOverLimit()
        {
            var huge = new string('a', Document.MaxContentLength + 1);

            Assert.Throws<DomainException>(() => Document.Create(FolderId, "Nota", huge, Priority.None, Now));
        }

        [Fact]
        public void Update_PublishesNewVersionAndMovesFolder()
        {
            var document = Document.Create(FolderId, "Nota", "<p>v1</p>", Priority.Low, Now);
            var otherFolder = ObjectIds.New();
            var later = Now.AddMinutes(30);

            document.Update(otherFolder, "Nota v2", "<p>v2</p>", Priority.Medium, later);

            Assert.Equal((otherFolder, "Nota v2", "<p>v2</p>", Priority.Medium, later),
                (document.FolderId, document.Title, document.Content, document.Priority, document.UpdatedOn));
            Assert.Equal(Now, document.CreatedOn);
        }

        [Fact]
        public void Type_DefaultsToRich_AndUpdateKeepsItUnlessChanged()
        {
            var nota = Document.Create(FolderId, "Nota", "<p>x</p>", Priority.None, Now);
            var pagina = Document.Create(FolderId, "Página", "<html><script>1</script></html>", Priority.None, Now, DocumentType.Html);

            Assert.Equal((DocumentType.Rich, DocumentType.Html), (nota.Type, pagina.Type));

            pagina.Update(FolderId, "Página v2", "<html></html>", Priority.Low, Now.AddMinutes(1));
            Assert.Equal(DocumentType.Html, pagina.Type);

            pagina.Update(FolderId, "Página v3", "<p>ahora nota</p>", Priority.Low, Now.AddMinutes(2), DocumentType.Rich);
            Assert.Equal(DocumentType.Rich, pagina.Type);
            Assert.Throws<DomainException>(() => Document.Create(FolderId, "x", "", Priority.None, Now, (DocumentType)9));
        }

        [Fact]
        public void Restore_ConvertsLocalDatesToUtc()
        {
            var local = new DateTime(2026, 5, 1, 10, 0, 0, DateTimeKind.Local);

            var document = Document.Restore(ObjectIds.New(), FolderId, "Nota", "", Priority.High, local, local);

            Assert.Equal(local.ToUniversalTime(), document.UpdatedOn);
            Assert.Equal(DateTimeKind.Utc, document.UpdatedOn.Kind);
        }
    }
}
