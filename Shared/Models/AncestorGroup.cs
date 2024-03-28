using System.Text.Json;
using System.Text.Json.Serialization;

namespace SOTM.Shared.Models {

    public class AncestorGroup<ChildType> : IIdentifiable
        where ChildType : class, IIdentifiable
    {
        public List<ChildType> JsonChildPropertyGetter() {
            return this.children.Values.ToList();
        }
        public void JsonChildPropertySetter(List<ChildType> other) {
            this.children.Clear();
            foreach (ChildType child in other) {
                this.AddChild(child);
            }
        }

        [JsonInclude]
        public GlobalIdentifier identifier;
        public Dictionary<string, ChildType> children = new Dictionary<string, ChildType>();
        public GlobalIdentifier GetIdentifier()
        {
            return identifier;
        }

        public AncestorGroup(GlobalIdentifier identifier)
        {
            this.identifier = identifier;
        }
        public ChildType? GetChild(GlobalIdentifier childIdentifier)
        {
            if (children.ContainsKey(childIdentifier.ToString())) return children[childIdentifier.ToString()];
            return null;
        }

        public ChildType AddChild(ChildType child)
        {
            children.TryAdd(child.GetIdentifier().ToString(), child);
            return child;
        }

        public void AddChildren(IEnumerable<ChildType> children)
        {
            foreach (ChildType child in children)
            {
                this.AddChild(child);
            }
        }

        public IEnumerable<ChildType> GetChildren()
        {
            return children.Values;
        }
    }
}
