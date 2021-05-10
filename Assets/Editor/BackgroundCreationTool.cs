using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BackgroundCreationTool : ScriptableWizard
{
    // Changes to layer will have to be hard-coded for now

    #region Variables
    [Header("Components")]
    [SerializeField] private Sprite _boxSprite;

    [Header("Sections")]
    [SerializeField] private Vector2 _sizeOfEachSection = new Vector2(3, 0);
    [SerializeField] private int _numberOfSections;

   [Header("Colors")]
    [SerializeField] private int _lengthOfFixedColorPattern = 2;
    [SerializeField] private List<Colors.colorName> _selectionOfColorsToRandomize = new List<Colors.colorName> {Colors.colorName.backgroundColorOne, Colors.colorName.backgroundColorTwo, Colors.colorName.backgroundColorThree, Colors.colorName.player, Colors.colorName.enemy};

    [Header("Layers")]
    private string _backgroundSectionsLayer = "Ignore Raycast";
    private string _backgroundSectionsTag = "Background";
    private string _spriteRendererSortingLayer = "Background";
    private string _spriteMaskSortingLayer = "Cubes";
    private string _levelConfinersFolderTag = "Confiners Folder";
    private string _cameraConfinerTag = "Camera Confiner";
    private string _playerConfinerTag = "Player Confiner";
    private string _deathColliderTag = "Death Collider";

    [Header("Background Sections Folder")]
    private Vector3 _backgroundSectionsFolderScale = new Vector3(1f, 1f, 1f);
    #endregion

    #region Unity Callbacks
    [MenuItem("My Tools/Background Creation Tool")]
    static void CreateWizard()
    {
        DisplayWizard<BackgroundCreationTool>("Create Background", "Create New");
    }

    private void OnWizardCreate()
    {
        GameObject backgroundSectionsFolder = CreateBackgroundSectionsFolder();

        var nextSectionPos = new Vector2(-0.5f * _sizeOfEachSection.x * (_numberOfSections - 1), 0);
        BackgroundColorShift previousSection = null;

        for (int i = 1; i < _numberOfSections + 1; i++)
        {
            GameObject section = null;

            if (previousSection == null)
                section = CreateBackgroundSection(i, backgroundSectionsFolder, null);
            else
                section = CreateBackgroundSection(i, backgroundSectionsFolder, previousSection);

            section.transform.position = nextSectionPos;
            nextSectionPos += new Vector2(_sizeOfEachSection.x, 0);

            previousSection = section.GetComponentInChildren<BackgroundColorShift>();
        }

        CreateLevelConfiners(backgroundSectionsFolder);
    }
    #endregion

    #region Background Sections Folder
    private GameObject CreateBackgroundSectionsFolder()
    {
        GameObject backgroundSectionsFolder = new GameObject();
        backgroundSectionsFolder.name = "Background Sections";
        backgroundSectionsFolder.transform.localScale = _backgroundSectionsFolderScale;

        return backgroundSectionsFolder;
    }
    #endregion

    #region Background Sections
    private GameObject CreateBackgroundSection(int sectionNumber, GameObject backgroundSectionsFolder, BackgroundColorShift previousSection)
    {
        GameObject backgroundSection = new GameObject();
        backgroundSection.name = "Section " + sectionNumber;

        backgroundSection.transform.localScale = new Vector3(_sizeOfEachSection.x, _sizeOfEachSection.y, 1);

        var sectionCollider = backgroundSection.AddComponent<BoxCollider2D>();
        sectionCollider.isTrigger = true;
        sectionCollider.size = new Vector2(0.98f, 1f);

        backgroundSection.layer = LayerMask.NameToLayer(_backgroundSectionsLayer);
        backgroundSection.tag = _backgroundSectionsTag;

        backgroundSection.transform.parent = backgroundSectionsFolder.transform; // Scale should be set after making child else scale is adjusted according to parent

        GameObject sectionSprite = CreateSectionSprite(sectionNumber, backgroundSection, previousSection);

        if (sectionNumber == 1)
            CreateSectionExtensionSprite(backgroundSection, sectionSprite, -_sizeOfEachSection.x / 2);
        else if (sectionNumber == _numberOfSections)
            CreateSectionExtensionSprite(backgroundSection, sectionSprite, _sizeOfEachSection.x / 2);

        return backgroundSection;
    }
    #endregion

    #region Background Sections Sprites
    private GameObject CreateSectionSprite(int sectionNumber, GameObject backgroundSection, BackgroundColorShift previousSection)
    {
        GameObject sectionSprite = new GameObject();
        sectionSprite.name = "Sprite";

        sectionSprite.transform.parent = backgroundSection.transform;

        var spriteRenderer = sectionSprite.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = _boxSprite;
        spriteRenderer.sortingLayerName = _spriteRendererSortingLayer;

        var wobblyComponent = sectionSprite.AddComponent<Wobbly>();
        if (sectionNumber % 2 == 1)
        {
            spriteRenderer.color = Color.black;
            wobblyComponent.invertWobble = false;
        }
        else
        {
            spriteRenderer.color = Color.yellow;
            wobblyComponent.invertWobble = true;
        }

        var spriteMask = sectionSprite.AddComponent<SpriteMask>();
        spriteMask.sprite = _boxSprite;
        spriteMask.isCustomRangeActive = true;
        spriteMask.frontSortingLayerID = SortingLayer.NameToID(_spriteMaskSortingLayer);
        spriteMask.frontSortingOrder = 5;
        spriteMask.backSortingLayerID = SortingLayer.NameToID(_spriteMaskSortingLayer);
        spriteMask.backSortingOrder = -5;

        var backgroundColorShiftComponent = sectionSprite.AddComponent<BackgroundColorShift>();
        
        for (int i = 0; i < _lengthOfFixedColorPattern; i++)
        {
            Colors.colorName randomColor;

            if (sectionNumber == 1) // Don't consider previous section color to create pattern
            {
                if (i == 0)
                {
                    int randomIndex = Random.Range(0, _selectionOfColorsToRandomize.Count);
                    randomColor = _selectionOfColorsToRandomize[randomIndex];
                }
                else if (i == _lengthOfFixedColorPattern)
                {
                    do
                    {
                        int randomIndex = Random.Range(0, _selectionOfColorsToRandomize.Count);
                        randomColor = _selectionOfColorsToRandomize[randomIndex];
                    } while (randomColor == backgroundColorShiftComponent._colorPattern[i - 1] || randomColor == backgroundColorShiftComponent._colorPattern[0]); // As 0 is the first element in the array
                }
                else
                {
                    do
                    {
                        int randomIndex = Random.Range(0, _selectionOfColorsToRandomize.Count);
                        randomColor = _selectionOfColorsToRandomize[randomIndex];
                    } while (randomColor == backgroundColorShiftComponent._colorPattern[i - 1]);
                }
            }
            else // Consider previous section color to create pattern
            {
                if (i == 0)
                {
                    do
                    {
                        int randomIndex = Random.Range(0, _selectionOfColorsToRandomize.Count);
                        randomColor = _selectionOfColorsToRandomize[randomIndex];
                    } while (randomColor == previousSection._colorPattern[i]);
                }
                else if (i == _lengthOfFixedColorPattern)
                {
                    do
                    {
                        int randomIndex = Random.Range(0, _selectionOfColorsToRandomize.Count);
                        randomColor = _selectionOfColorsToRandomize[randomIndex];
                    } while (randomColor == backgroundColorShiftComponent._colorPattern[i - 1] || randomColor == backgroundColorShiftComponent._colorPattern[0] ||
                    randomColor == previousSection._colorPattern[i]); // As 0 is the first element in the array
                }
                else
                {
                    do
                    {
                        int randomIndex = Random.Range(0, _selectionOfColorsToRandomize.Count);
                        randomColor = _selectionOfColorsToRandomize[randomIndex];
                    } while (randomColor == backgroundColorShiftComponent._colorPattern[i - 1] || randomColor == previousSection._colorPattern[i]);
                }
            }

            backgroundColorShiftComponent._colorPattern.Add(randomColor);
        }

        sectionSprite.transform.localScale = Vector3.one; // Scale is set to zero due to a glitch if it is set before adding wobbly component

        return sectionSprite;
    }
    #endregion

    #region Background Section Sprites Extension
    private void CreateSectionExtensionSprite(GameObject backgroundSection, GameObject sectionSprite, float xOffsetOfExtension)
    {
        GameObject sectionExtensionSprite = new GameObject();
        sectionExtensionSprite.name = "Sprite Extension";

        sectionExtensionSprite.transform.parent = backgroundSection.transform;

        var spriteRenderer = sectionExtensionSprite.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = _boxSprite;
        spriteRenderer.color = Color.red;
        spriteRenderer.sortingLayerName = _spriteRendererSortingLayer;
        spriteRenderer.sortingOrder = -1;

        var spriteMask = sectionExtensionSprite.AddComponent<SpriteMask>();
        spriteMask.sprite = _boxSprite;
        spriteMask.isCustomRangeActive = true;
        spriteMask.frontSortingLayerID = SortingLayer.NameToID(_spriteMaskSortingLayer);
        spriteMask.frontSortingOrder = 5;
        spriteMask.backSortingLayerID = SortingLayer.NameToID(_spriteMaskSortingLayer);
        spriteMask.backSortingOrder = -5;

        sectionExtensionSprite.transform.localScale = Vector3.one;
        sectionExtensionSprite.transform.Translate(Vector3.right * xOffsetOfExtension);
    }
    #endregion

    #region Confiners and Colliders
    private void CreateLevelConfiners(GameObject backgroundSectionsFolder)
    {
        GameObject levelConfinersFolder = new GameObject();
        levelConfinersFolder.name = "Level Confiners";
        levelConfinersFolder.tag = _levelConfinersFolderTag;

        levelConfinersFolder.transform.parent = backgroundSectionsFolder.transform;

        CreateCameraConfiner(levelConfinersFolder);
        CreatePlayerConfiner(levelConfinersFolder);
        CreateDeathCollider(levelConfinersFolder);
    }

    private void CreateCameraConfiner(GameObject levelConfinersFolder)
    {
        GameObject cameraConfiner = new GameObject();
        cameraConfiner.name = "Camera Confiner";
        cameraConfiner.tag = _cameraConfinerTag;

        cameraConfiner.transform.parent = levelConfinersFolder.transform;

        BoxCollider2D confinerCollider = cameraConfiner.AddComponent<BoxCollider2D>();
        confinerCollider.usedByComposite = true;
        confinerCollider.size = new Vector2(_sizeOfEachSection.x * _numberOfSections, _sizeOfEachSection.y);

        Rigidbody2D confinerRb = cameraConfiner.AddComponent<Rigidbody2D>();
        confinerRb.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D confinerCompositeCollider = cameraConfiner.AddComponent<CompositeCollider2D>();
        confinerCompositeCollider.isTrigger = true;
        confinerCompositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
    }

    private void CreatePlayerConfiner(GameObject levelConfinersFolder)
    {
        GameObject playerConfiner = new GameObject();
        playerConfiner.name = "Player Confiner";
        playerConfiner.tag = _playerConfinerTag;

        playerConfiner.transform.parent = levelConfinersFolder.transform;

        EdgeCollider2D confinerCollider = playerConfiner.AddComponent<EdgeCollider2D>();
        confinerCollider.usedByEffector = true;

        List<Vector2> confinerPoints = new List<Vector2>();
        confinerPoints.Add(new Vector2((-_sizeOfEachSection.x * _numberOfSections) / 2, -(_sizeOfEachSection.y + 2) / 2));
        confinerPoints.Add(new Vector2((-_sizeOfEachSection.x * _numberOfSections) / 2, _sizeOfEachSection.y / 2));
        confinerPoints.Add(new Vector2((_sizeOfEachSection.x * _numberOfSections) / 2, _sizeOfEachSection.y / 2));
        confinerPoints.Add(new Vector2((_sizeOfEachSection.x * _numberOfSections) / 2, -(_sizeOfEachSection.y + 2) / 2));

        confinerCollider.points = confinerPoints.ToArray();

        PlatformEffector2D confinerEffector = playerConfiner.AddComponent<PlatformEffector2D>();
        confinerEffector.useOneWay = false;
    }

    private void CreateDeathCollider(GameObject levelConfinersFolder)
    {
        GameObject deathCollider = new GameObject();
        deathCollider.name = "Death Collider";

        deathCollider.transform.parent = levelConfinersFolder.transform;

        deathCollider.tag = _deathColliderTag;

        BoxCollider2D colliderComponent = deathCollider.AddComponent<BoxCollider2D>();
        colliderComponent.isTrigger = true;
        colliderComponent.size = new Vector2(_sizeOfEachSection.x * _numberOfSections, 1);
        colliderComponent.offset = new Vector2(0, - (_sizeOfEachSection.y + 2) / 2);
    }
    #endregion
}
