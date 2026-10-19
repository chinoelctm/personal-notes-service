using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Tests.Domain
{
    public class FolderTests
    {
        private static readonly DateTime Now = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void Create_TrimsAndNormalizesName()
        {
            var folder = Folder.Create("  Trabajo & Empresa ", null, Now);

            Assert.Equal(("Trabajo & Empresa", "trabajo & empresa", true),
                (folder.Name, folder.NormalizedName, folder.IsRoot));
            Assert.True(ObjectIds.IsValid(folder.Id));
            Assert.Equal((Now, Now), (folder.CreatedOn, folder.UpdatedOn));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_RejectsEmptyName(string name)
        {
            Assert.Throws<DomainException>(() => Folder.Create(name, null, Now));
        }

        [Fact]
        public void Create_RejectsTooLongName()
        {
            Assert.Throws<DomainException>(() => Folder.Create(new string('a', Folder.MaxNameLength + 1), null, Now));
        }

        [Fact]
        public void Create_RejectsInvalidParent_AndTreatsBlankParentAsRoot()
        {
            Assert.Throws<DomainException>(() => Folder.Create("Sprints", "no-es-un-id", Now));
            Assert.True(Folder.Create("Sprints", "  ", Now).IsRoot);
        }

        [Fact]
        public void Update_CannotBeItsOwnParent()
        {
            var folder = Folder.Create("Sprints", null, Now);

            Assert.Throws<BusinessRuleException>(() => folder.Update("Sprints", folder.Id, Now));
        }

        [Fact]
        public void Update_RenamesMovesAndStampsUpdatedOn()
        {
            var folder = Folder.Create("Sprints", null, Now);
            var parent = ObjectIds.New();
            var later = Now.AddMinutes(5);

            folder.Update("Sprints Q3", parent, later);

            Assert.Equal(("Sprints Q3", "sprints q3", parent, later),
                (folder.Name, folder.NormalizedName, folder.ParentFolderId, folder.UpdatedOn));
            Assert.Equal(Now, folder.CreatedOn);
        }

        [Fact]
        public void Restore_KeepsIdAndTreatsUnspecifiedDatesAsUtc()
        {
            var id = ObjectIds.New();
            var unspecified = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified);

            var folder = Folder.Restore(id, "Raíz", null, unspecified, unspecified);

            Assert.Equal(id, folder.Id);
            Assert.Equal(DateTimeKind.Utc, folder.CreatedOn.Kind);
            Assert.Equal(unspecified.Ticks, folder.CreatedOn.Ticks);
        }
    }
}
