using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextController : MonoBehaviour
{
    [SerializeReference] private TMP_Text textComponent;
    TMP_TextInfo textInfo;

    [SerializeField] string fullText;
    private string simpleText;

    int typingTextCounter = 0;
    float typeTimeChecker = 0;

    [SerializeField] private List<EffectReader> effectReaders;

    [SerializeField] private TextManager txetMana;

    private float[] typingProgress;

    [SerializeField] private bool hasTextChanged;
    private bool DoTextChanged;

    private System.Random rand = new System.Random();


    void Start()
    {
        textInfo = textComponent.textInfo;
        TextChanged("\\<test> <'' 0.5><~>wave</~><''> and <!>impact plus <*>shake</!></*> + <%>r o t a t e</%> ");
        StartCoroutine(TextAnimation());
        StartCoroutine(TextTyping());
    }

    void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    }

    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }

    void ON_TEXT_CHANGED(Object obj)
    {
        if (obj == textComponent)
            hasTextChanged = true;
    }

    void TextChanged(string newText)
    {
        if (fullText == newText)
        {
            return;
        }

        DoTextChanged = true;

        fullText = newText;
        simpleText = ToTmpText(fullText);

        effectReaders.Clear();
        typingTextCounter = 0;
        typeTimeChecker = 0;

        int skipCount = 0;
        float textSpeed = txetMana.typingDelay;

        for (int i = 0; i + skipCount < fullText.Length; i++)
        {
            effectReaders.Add(new EffectReader());
            if (i == 0 || fullText[i + skipCount - 1] != '\\')
            {
                while (i + skipCount < fullText.Length - 1 && fullText[i + skipCount] == '<')
                {
                    string tagText = string.Empty;
                    for (int j = 0; fullText[i + skipCount + j] != '>'; j++)
                    {
                        tagText += fullText[i + skipCount + j];
                    }
                    
                    try
                    {
                        if (tagText.Substring(0, 3) == "<\'\'")
                        {
                            if (float.TryParse(tagText.Remove(0, 3), out float floatCheck))
                            {
                                textSpeed = floatCheck;
                            }
                            else
                            {
                                textSpeed = txetMana.typingDelay;
                            }
                        }
                    }
                    catch (System.Exception) { }
                    

                    tagText += '>';

                    effectReaders[i].effects.Add(tagText);

                    skipCount += tagText.Length;
                }
            }
            else if (fullText[i + skipCount] == '<')
            {
                skipCount++;
            }

            effectReaders[i].textSpeed = textSpeed;
        }

        textComponent.text = simpleText.Replace("\\<","<");
    }

    IEnumerator TextAnimation()
    {
        textComponent.ForceMeshUpdate();
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        typingProgress = new float[textInfo.characterCount];

        float shakeTimer = 0;
        int randomSeed = Random.Range(0, 1001);

        while (true)
        {
            bool wave = false, shake = false, impact = false, rotate = false;
            
            if (DoTextChanged)
            {
                yield return new WaitUntil(() => DoTextChanged && hasTextChanged);
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
                typingProgress = new float[textInfo.characterCount];

                hasTextChanged = DoTextChanged = false;
            }
            
            int characterCount = textInfo.characterCount;
            
            rand = new System.Random(randomSeed);
            
            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                
                foreach (string tagText in effectReaders[i].effects)
                {
                    switch (tagText)//tag apply
                    {
                        case "<~>":
                            wave = true;
                            break;
                        case "</~>":
                            wave = false;
                            break;
                        case "<*>":
                            shake = true;
                            break;
                        case "</*>":
                            shake = false;
                            break;
                        case "<!>":
                            impact = true;
                            break;
                        case "</!>":
                            impact = false;
                            break;
                        case "<%>":
                            rotate = true;
                            break;
                        case "</%>":
                            rotate = false;
                            break;
                        default:
                            break;
                    }
                }
                
                if (!charInfo.isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;
                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                for (int j = 0; j < 4; j++)//rest vertices position
                {
                    destinationVertices[vertexIndex + j] = sourceVertices[vertexIndex + j];
                }

                if (shake)//shake animation
                {
                    Vector3 randpos = new Vector3(NextFloat(-txetMana.shakePower, txetMana.shakePower), NextFloat(-txetMana.shakePower, txetMana.shakePower), 0);
                    for (int j = 0; j < 4; j++)
                    {
                        destinationVertices[vertexIndex + j] += randpos;
                    }
                }

                if (wave)//wave animation
                {
                    Vector3 sinpos = new Vector3(0, Mathf.Sin(i * txetMana.waveGap - Time.time * txetMana.waveSpeed) * txetMana.wavePower, 0);
                    for (int j = 0; j < 4; j++)
                    {
                        destinationVertices[vertexIndex + j] += sinpos;
                    }
                }

                if (rotate)//wave animation
                {
                    Vector3 center = Vector3.Lerp(destinationVertices[vertexIndex], destinationVertices[vertexIndex + 2],0.5f);
                    float rad = Mathf.Sin(1 / Mathf.Sin(i * txetMana.rotateRandomA) + 1 / Mathf.Cos(i * txetMana.rotateRandomB)) * txetMana.rotateAngle * Mathf.Deg2Rad;
                    
                    for (int j = 0; j < 4; j++)
                    {
                        Vector3 dir = new Vector3(destinationVertices[vertexIndex + j].x - center.x, destinationVertices[vertexIndex + j].y - center.y);
                        Vector3 pos = new Vector3(Mathf.Cos(rad) * dir.x - Mathf.Sin(rad) * dir.y, Mathf.Sin(rad) * dir.x + Mathf.Cos(rad) * dir.y);

                        destinationVertices[vertexIndex + j] += pos - dir;
                    }

                }

                Vector3 typingPos;
                Vector3 scaleDistance0;
                Vector3 scaleDistance1;

                if (impact)//text typing animation
                {
                    typingPos = txetMana.impactTypingTransform.position;
                    scaleDistance0 = Vector3.Scale((destinationVertices[vertexIndex + 2] - destinationVertices[vertexIndex]) / 2, txetMana.impactTypingTransform.Scale - Vector3.one);
                    scaleDistance1 = Vector3.Scale((destinationVertices[vertexIndex + 3] - destinationVertices[vertexIndex + 1]) / 2, txetMana.impactTypingTransform.Scale - Vector3.one);
                }
                else
                {
                    typingPos = txetMana.typingTransform.position;
                    scaleDistance0 = Vector3.Scale((destinationVertices[vertexIndex + 2] - destinationVertices[vertexIndex]) / 2, txetMana.typingTransform.Scale - Vector3.one);
                    scaleDistance1 = Vector3.Scale((destinationVertices[vertexIndex + 3] - destinationVertices[vertexIndex + 1]) / 2, txetMana.typingTransform.Scale - Vector3.one);
                }

                for (int j = 0; j < 4; j++)//typing position
                {
                    destinationVertices[vertexIndex + j] += Vector3.Lerp(typingPos, Vector3.zero, typingProgress[i]);
                }

                destinationVertices[vertexIndex] += Vector3.Lerp(Vector3.Scale(scaleDistance0, new Vector3(-1, -1, 1)), Vector3.zero, typingProgress[i]);//typing scale
                destinationVertices[vertexIndex + 1] += Vector3.Lerp(Vector3.Scale(scaleDistance1, new Vector3(-1, -1, 1)), Vector3.zero, typingProgress[i]);
                destinationVertices[vertexIndex + 2] += Vector3.Lerp(Vector3.Scale(scaleDistance0, new Vector3(1, 1, 1)), Vector3.zero, typingProgress[i]);
                destinationVertices[vertexIndex + 3] += Vector3.Lerp(Vector3.Scale(scaleDistance1, new Vector3(1, 1, 1)), Vector3.zero, typingProgress[i]);
                
            }
            
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
            
            shakeTimer += Time.deltaTime;

            if (shakeTimer >= txetMana.shakeDelay)
            {
                randomSeed = Random.Range(0,1001);
                shakeTimer -= txetMana.shakeDelay;
            }
            
            yield return null;
        }
    }

    IEnumerator TextTyping()
    {
        Color32[] newVertexColors;

        while (true)
        {
            int characterCount = textInfo.characterCount;

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible)
                    continue;

                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                newVertexColors = textInfo.meshInfo[materialIndex].colors32;


                if (typingTextCounter >= i)
                {
                    typingProgress[i] += Time.deltaTime / txetMana.typingSpeed;
                }

                for (int j = 0; j < 4; j++)
                {
                    newVertexColors[vertexIndex + j] = Color32.Lerp(txetMana.typingTransform.color[j], txetMana.baseColor[j], typingProgress[i]);
                }

                textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            }

            typeTimeChecker += Time.deltaTime;
            try
            {
                if (typeTimeChecker >= effectReaders[typingTextCounter].textSpeed && typingTextCounter < characterCount)
                {
                    typeTimeChecker -= effectReaders[typingTextCounter].textSpeed;
                    typingTextCounter++;
                }
            }
            catch (System.Exception) { }

            yield return null;
        }
        
    }

    private string ToTmpText(string originalText)
    {
        foreach (TagReference tag in txetMana.changeTags)
        {
            originalText = originalText.Replace(tag.oldTag, tag.newText);
        }

        foreach (string tag in txetMana.scriptTags)
        {
            originalText = originalText.Replace($"<{tag}>", string.Empty);
            originalText = originalText.Replace($"</{tag}>", string.Empty);
        }
        
        string tagText = null;
        for (int i = originalText.Length - 1; i >= 0; i--)
        {
            if (i == 0 || originalText[i - 1] != '\\')
            {
                if (originalText[i] == '>')
                {
                    tagText = string.Empty;
                }
                else if (originalText[i] == '<')
                {
                    try
                    {
                        if (tagText.Substring(0, 3) == "\'\' ")
                        {
                            if (float.TryParse(tagText.Remove(0, 3), out float floatCheck))
                            {
                                originalText = originalText.Remove(i, tagText.Length + 2);
                            }
                        }
                    }
                    catch (System.Exception) { }
                    tagText = null;
                }
                else if(tagText != null)
                {
                    tagText = $"{originalText[i]}{tagText}";
                }
            }
        }

        return originalText;
    }

    private float NextFloat(float min, float max)
    {
        double val = rand.NextDouble() * (max - min) + min;
        return (float)val;
    }

    [System.Serializable]
    class EffectReader
    {
        public float textSpeed;
        public List<string> effects = new List<string>();
    }
}
