using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Exceptions;

namespace PersonalNotes.Tests.Application
{
    public class FolderHierarchyTests
    {
        private static readonly DateTime Now = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

        // Raíz > Trabajo > Sprints Q3 > Arquitectura   |   Personal
        private readonly Folder _root = Folder.Create("Raíz", null, Now);
        private readonly Folder _personal = Folder.Create("Personal", null, Now);
        private readonly Folder _work;
        private readonly Folder _sprints;
        private readonly Folder _architecture;
        private readonly FolderHierarchy _hierarchy;

        public FolderHierarchyTests()
        {
            _work = Folder.Create("Trabajo", _root.Id, Now);
            _sprints = Folder.Create("Sprints Q3", _work.Id, Now);
            _architecture = Folder.Create("Arquitectura", _sprints.Id, Now);
            _hierarchy = new FolderHierarchy([_root, _personal, _work, _sprints, _architecture]);
        }

        [Fact]
        public void DepthAndPath_StartAtTheRoot()
        {
            Assert.Equal(1, _hierarchy.DepthOf(_root.Id));
            Assert.Equal(4, _hierarchy.DepthOf(_architecture.Id));
            Assert.Equal(["Raíz", "Trabajo", "Sprints Q3", "Arquitectura"],
                _hierarchy.PathOf(_architecture.Id).Select(f => f.Name));
        }

        [Fact]
        public void Subtree_IncludesTheFolderAndAllDescendants()
        {
            var subtree = _hierarchy.SubtreeOf(_work.Id);

            Assert.Equal(new[] { _work.Id, _sprints.Id, _architecture.Id }.Order(), subtree.Order());
            Assert.Equal(3, _hierarchy.HeightOf(_work.Id));
            Assert.Equal(1, _hierarchy.HeightOf(_personal.Id));
        }

        [Fact]
        public void EnsureCanPlace_AllowsAtLeastThreeLevels()
        {
            var hierarchy = new FolderHierarchy([_root, _work]);

            // Nueva carpeta en el nivel 3 con un límite de 3
            hierarchy.EnsureCanPlace(folderId: null, _work.Id, maxDepth: 3);
        }

        [Fact]
        public void EnsureCanPlace_RejectsNewFolderBeyondMaxDepth()
        {
            Assert.Throws<BusinessRuleException>(() =>
                _hierarchy.EnsureCanPlace(folderId: null, _architecture.Id, maxDepth: 4));
            _hierarchy.EnsureCanPlace(folderId: null, _architecture.Id, maxDepth: 5);
        }

        [Fact]
        public void EnsureCanPlace_CountsTheWholeSubtreeWhenMoving()
        {
            // Trabajo ocupa 3 niveles: bajo Personal (nivel 1) llegaría al nivel 4
            Assert.Throws<BusinessRuleException>(() => _hierarchy.EnsureCanPlace(_work.Id, _personal.Id, maxDepth: 3));
            _hierarchy.EnsureCanPlace(_work.Id, _personal.Id, maxDepth: 4);
        }

        [Fact]
        public void EnsureCanPlace_RejectsMovingIntoItsOwnSubtree()
        {
            Assert.Throws<BusinessRuleException>(() => _hierarchy.EnsureCanPlace(_work.Id, _architecture.Id, maxDepth: 10));
        }

        [Fact]
        public void EnsureCanPlace_RejectsUnknownParent()
        {
            Assert.Throws<NotFoundException>(() =>
                _hierarchy.EnsureCanPlace(folderId: null, Folder.Create("Otra", null, Now).Id, maxDepth: 5));
        }

        [Fact]
        public void FolderWithMissingParent_IsTreatedAsRoot()
        {
            var orphan = Folder.Create("Huérfana", _personal.Id, Now);
            var hierarchy = new FolderHierarchy([orphan]);

            Assert.Equal(1, hierarchy.DepthOf(orphan.Id));
        }
    }
}
