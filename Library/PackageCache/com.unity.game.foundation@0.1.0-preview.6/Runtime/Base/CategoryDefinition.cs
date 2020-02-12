using System;

namespace UnityEngine.GameFoundation
{
    /// <summary>
    /// CategoryDefinition describes a Category.
    /// CategoryDefinitions are usable across multiple systems, but each system is 
    /// responsible for containing and managing its own Categories.
    /// </summary>
    [System.Serializable]
    public class CategoryDefinition
    {
        [SerializeField]
        private string m_Id;

        /// <summary>x
        /// The string Id of this CategoryDefinition.
        /// </summary>
        /// <returns>The string Id of this CategoryDefinition.</returns>
        public string id
        {
            get { return m_Id; }
        }

        [SerializeField] 
        private int m_Hash;

        /// <summary>
        /// The Hash of this CategoryDefinition.
        /// </summary>
        /// <returns>The Hash of this CategoryDefinition.</returns>
        public int hash
        {
            get { return m_Hash; }
        }

        [SerializeField] 
        private string m_DisplayName;

        /// <summary>
        /// The name of this CategoryDefinition for the user to display.
        /// </summary>
        /// <returns>The name of this CategoryDefinition for the user to display.</returns>
        public string displayName
        {
            get { return m_DisplayName; }
            set { m_DisplayName = value; }
        }

        /// <summary>
        /// Constructor for a CategoryDefinition.
        /// </summary>
        /// <param name="id">The Id this CategoryDefinition will use.</param>
        /// <param name="displayName">The name this CategoryDefinition will use.</param>
        /// <exception cref="ArgumentException">Thrown if an empty Id is given.</exception>
        public CategoryDefinition(string id, string displayName)
        {
            Tools.ThrowIfPlayMode("Cannot create a CategoryDefinition while in play mode.");
            
            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentException("CategoryDefinition cannot have null or empty id.");
            }
            
            if (!Tools.IsValidId(id))
            {
                throw new System.ArgumentException("CategoryDefinition can only be alphanumeric with optional dashes or underscores.");
            }
            
            if (string.IsNullOrEmpty(displayName))
            {
                throw new System.ArgumentException("CategoryDefinition cannot have null or empty display name.");
            }
            
            m_DisplayName = displayName;
            m_Id = id;
            m_Hash = Tools.StringToHash(m_Id); 
        }

        /// <summary>
        /// == Overload. If ID and DisplayName match then the CategoryDefinitions are deemed equal.
        /// Note: Two null objects are considered equal.
        /// </summary>
        /// <param name="x">A CategoryDefinition to compare.</param>
        /// <param name="y">A CategoryDefinition to compare.</param>
        /// <returns>True if both CategoryDefinitions are the same (id & display name must match).</returns>
        public static bool operator ==(CategoryDefinition x, CategoryDefinition y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.id == y.id && x.displayName == y.displayName;
        }

        /// <summary>
        /// != Overload. If ID and DisplayName don't match the CategoryDefinitions are deemed not equal.
        /// Note: Two null objects are considered equal.
        /// </summary>
        /// <param name="x">A CategoryDefinition to compare.</param>
        /// <param name="y">A CategoryDefinition to compare.</param>
        /// <returns>False if both CategoryDefinitions are the same (id & display name's both match).</returns>
        public static bool operator !=(CategoryDefinition x, CategoryDefinition y)
        {
            return !(x == y);
        }

        /// <summary>
        /// If ID and DisplayName match then the CategoryDefinitions are deemed equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>True if both CategoryDefinitions are the same (id & display name must match).</returns>
        public override bool Equals(object obj)
        {
            var categoryDefinition = obj as CategoryDefinition;
            return categoryDefinition != null && this == categoryDefinition;
        }

        /// <summary>
        /// Returns the Hash code associated with this CategoryDefinition's Id.
        /// </summary>
        /// <returns>The Hash code associated with this CategoryDefinition's Id.</returns>
        public override int GetHashCode()
        {
            return m_Hash;
        }
    }
    
}
