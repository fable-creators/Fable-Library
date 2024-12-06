private void ApplyImageToPage(Texture2D texture)
{
    bool isFrontPage = (currentPageIndex % 2) == 0;
    int pageParamIndex = currentPageIndex / 2;

    while (bookComponent.pageparams.Count <= pageParamIndex)
    {
        bookComponent.pageparams.Add(new MegaBookPageParams());
    }

    var pageParam = bookComponent.pageparams[pageParamIndex];

    // Create new material based on the book's current page material to maintain shader properties
    Material newMaterial = null;
    if (isFrontPage)
    {
        if (pageParam.frontmat != null)
        {
            newMaterial = new Material(pageParam.frontmat);
        }
        else
        {
            // If no existing material, copy from another page or use default
            newMaterial = new Material(bookComponent.pageparams[0].frontmat != null ? 
                bookComponent.pageparams[0].frontmat : defaultMaterial);
        }
        
        pageParam.front = texture;
        pageParam.frontmat = newMaterial;
        newMaterial.mainTexture = texture;
        newMaterial.SetTexture("_MainTex", texture);
        newMaterial.SetTexture("_BaseMap", texture);
        
        // Ensure material properties are copied
        if (bookComponent.pageparams[0].frontmat != null)
        {
            newMaterial.CopyPropertiesFromMaterial(bookComponent.pageparams[0].frontmat);
            newMaterial.mainTexture = texture; // Re-apply texture after copy
        }
    }
    else
    {
        if (pageParam.backmat != null)
        {
            newMaterial = new Material(pageParam.backmat);
        }
        else
        {
            newMaterial = new Material(bookComponent.pageparams[0].backmat != null ? 
                bookComponent.pageparams[0].backmat : defaultMaterial);
        }
        
        pageParam.back = texture;
        pageParam.backmat = newMaterial;
        newMaterial.mainTexture = texture;
        newMaterial.SetTexture("_MainTex", texture);
        newMaterial.SetTexture("_BaseMap", texture);
        
        // Ensure material properties are copied
        if (bookComponent.pageparams[0].backmat != null)
        {
            newMaterial.CopyPropertiesFromMaterial(bookComponent.pageparams[0].backmat);
            newMaterial.mainTexture = texture; // Re-apply texture after copy
        }
    }

    // Force rebuild
    bookComponent.rebuildmeshes = true;
    bookComponent.rebuild = true;
    bookComponent.updateBindings = true;
    
    // Force material update
    if (newMaterial != null)
    {
        newMaterial.SetFloat("_Mode", 0); // Opaque mode
        newMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        newMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        newMaterial.SetInt("_ZWrite", 1);
        newMaterial.DisableKeyword("_ALPHATEST_ON");
        newMaterial.DisableKeyword("_ALPHABLEND_ON");
        newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        newMaterial.renderQueue = -1;
    }
} 