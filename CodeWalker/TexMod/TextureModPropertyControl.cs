using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.TexMod;

public partial class TextureModPropertyControl : DockContent
{
    public TextureModPropertyControl()
    {
        InitializeComponent();
    }

    TextureModProject project => mainForm.project;
    public TextureModDockForm mainForm;
    public Action onPropertyGridChanged;

    public void SelectObject(TextureMapping mapping)
    {
        if (mapping == null)
        {
            propertyGrid.SelectedObject = null;
            return;
        }
        propertyGrid.SelectedObject = PropertyObject.Create(project, mapping, onPropertyGridChanged);
    }

    class PropertyObject
    {
        [ReadOnly(true)]
        public string id { get; set; }

        public TextureLod lod
        {
            get => sourceObject.lod;
            set
            {
                sourceObject.lod = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        public string name
        {
            get => sourceObject.name;
            set
            {
                sourceObject.name = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        [ReadOnly(true)]
        public string modTexture { get; set; }

        [ReadOnly(true)]
        public string sourceTexture { get; set; }

        public string targetRect { get; set; }

        public bool flipX
        {
            get => sourceObject.flipX;
            set
            {
                sourceObject.flipX = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        public bool flipY
        {
            get => sourceObject.flipY;
            set
            {
                sourceObject.flipY = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        public bool swap
        {
            get => sourceObject.swap;
            set
            {
                sourceObject.swap = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        private TextureMapping sourceObject;
        private Action onPropertyGridChanged;

        public static PropertyObject Create(TextureModProject project, TextureMapping mapping, Action onPropertyGridChanged)
        {
            var propertyObject = new PropertyObject();
            if (project.sourceTextures.TryGetValue(mapping.sourceTexture, out var sourceTexture))
            {
                propertyObject.sourceTexture = sourceTexture.sourceFile;
            }
            if (project.modTextures.TryGetValue(mapping.modTexture, out var modTexture))
            {
                propertyObject.modTexture = modTexture.filename;
            }
            propertyObject.id = mapping.id.ToString("N");
            propertyObject.sourceObject = mapping;
            propertyObject.onPropertyGridChanged = onPropertyGridChanged;
            return propertyObject;
        }
    }
}