using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Exceptions;

namespace PersonalNotes.Application.Services.Folders
{
    /// <summary>
    /// Árbol de carpetas activas en memoria: profundidad, ruta, descendientes y validación de movimientos.
    /// Una carpeta raíz es el nivel 1. Una carpeta cuyo padre no está en el árbol (inactivo o inexistente)
    /// se trata como raíz, para que un dato inconsistente no rompa el listado.
    /// </summary>
    public class FolderHierarchy
    {
        private readonly Dictionary<string, Folder> _byId;
        private readonly ILookup<string, Folder> _children;

        public FolderHierarchy(IEnumerable<Folder> folders)
        {
            _byId = folders.ToDictionary(f => f.Id);
            _children = _byId.Values
                .Where(f => f.ParentFolderId is not null && _byId.ContainsKey(f.ParentFolderId))
                .ToLookup(f => f.ParentFolderId!);
        }

        public IReadOnlyCollection<Folder> Folders => _byId.Values;

        public bool Contains(string id) => _byId.ContainsKey(id);

        /// <summary>Nivel de la carpeta: 1 para una raíz, 2 para su subcarpeta, etc.</summary>
        public int DepthOf(string id) => PathOf(id).Count;

        /// <summary>Carpetas desde la raíz hasta <paramref name="id"/> (incluida).</summary>
        public IReadOnlyList<Folder> PathOf(string id)
        {
            var path = new List<Folder>();
            var visited = new HashSet<string>();
            var current = Get(id);

            while (current is not null && visited.Add(current.Id))
            {
                path.Add(current);
                current = current.ParentFolderId is null ? null : _byId.GetValueOrDefault(current.ParentFolderId);
            }

            path.Reverse();
            return path;
        }

        /// <summary>La carpeta y todas sus subcarpetas, a cualquier profundidad.</summary>
        public IReadOnlyList<string> SubtreeOf(string id)
        {
            var result = new List<string>();
            var visited = new HashSet<string>();
            var pending = new Stack<string>([Get(id).Id]);

            while (pending.Count > 0)
            {
                var current = pending.Pop();
                if (!visited.Add(current)) continue;
                result.Add(current);
                foreach (var child in _children[current])
                    pending.Push(child.Id);
            }

            return result;
        }

        /// <summary>Niveles que ocupa el subárbol: 1 si no tiene subcarpetas.</summary>
        public int HeightOf(string id)
        {
            var visited = new HashSet<string>();
            return Height(Get(id).Id, visited);
        }

        /// <summary>
        /// Valida que una carpeta nueva o movida pueda colgar de <paramref name="parentId"/> sin superar
        /// <paramref name="maxDepth"/> niveles ni crear un ciclo (mover una carpeta dentro de sí misma).
        /// </summary>
        /// <param name="folderId">null para una carpeta nueva.</param>
        public void EnsureCanPlace(string? folderId, string? parentId, int maxDepth)
        {
            if (parentId is not null && !Contains(parentId))
                throw new NotFoundException("La carpeta padre no existe.");

            if (folderId is not null && parentId is not null && SubtreeOf(folderId).Contains(parentId))
                throw new BusinessRuleException("No se puede mover una carpeta dentro de sí misma ni de sus subcarpetas.");

            var parentDepth = parentId is null ? 0 : DepthOf(parentId);
            var height = folderId is null ? 1 : HeightOf(folderId);

            if (parentDepth + height > maxDepth)
                throw new BusinessRuleException(
                    $"El árbol admite hasta {maxDepth} niveles de carpetas; esta ubicación lo superaría.");
        }

        private int Height(string id, HashSet<string> visited)
        {
            if (!visited.Add(id)) return 0;
            var children = _children[id].ToList();
            return 1 + (children.Count == 0 ? 0 : children.Max(c => Height(c.Id, visited)));
        }

        private Folder Get(string id) =>
            _byId.GetValueOrDefault(id) ?? throw new NotFoundException("La carpeta no existe.");
    }
}
