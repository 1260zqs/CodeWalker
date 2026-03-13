using System.Collections.Generic;
using CodeWalker.TexMod;

namespace CodeWalker.Tools;

public partial class TextureModForm : ITextureModAdapter
{
    public TextureModProject project;
    public TextureModAdapter adapter;

    private ModTexture currentMod;
    private List<TextureReplacement> replacements = new();

    private void OnSelectTexMod(ModTexture modTexture)
    {
        currentMod = modTexture;
        replacements.Clear();
        if (currentMod != null)
        {
            project.FindTextureReplacements(currentMod.id, replacements);
            DisplayPicture(previewPictureBoxAsync, currentMod.filename);
            previewPictureBox.ResetViewer();
        }
        RefreshReplacementListView();
    }
}