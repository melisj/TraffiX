using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Traffic
{
    class ObjectList : GameObject
    {
        public List<GameObject> children;

        public ObjectList()
        {
            children = new List<GameObject>();
        }

        // de lijst van children aanpassen in de lijst
        public void AddRemove(GameObject gameObject, String addOrRemove)
        {
            if (addOrRemove == "add")
                children.Add(gameObject);
            else if (addOrRemove == "remove")
                children.Remove(gameObject);
        }

        public void RemoveAll()
        {
            for (int iChild = children.Count - 1; iChild >= 0; iChild--)
            {
                children.RemoveAt(iChild);
            }
        }


        public override void Update(GameTime gameTime)
        {
                base.Update(gameTime);
            foreach (GameObject gameObject in children)
            {
                gameObject.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach (GameObject gameObject in children)
            {
                gameObject.Draw(spriteBatch);
            }
        }
    }
}
