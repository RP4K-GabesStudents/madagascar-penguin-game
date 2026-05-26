using System;
using GabesCommonUtility.Extensions;
using TMPro;
using UnityEngine;

public class CopyPasteClipboard : MonoBehaviour
{
    [SerializeField] private TMP_InputField pasteField;
    [SerializeField] private TextMeshProUGUI copyField;

    [SerializeField] private bool tryAutoPaste;
    
    private string _text;

    private void Awake()
    {
        if (tryAutoPaste && pasteField)
        {
            pasteField.onSelect.AddListener(PasteAuto);
        }
    }
    
    private void PasteAuto(string arg0)
    {
        if (pasteField.text.Length > 0) return;
        Paste();
    }

    public void Copy()
    {
        _text = copyField.text;
        UniClipboard.SetText(_text);
    }

    public void Paste()
    {
        var text = UniClipboard.GetText();
        if (text.Length > pasteField.characterLimit) return;
        _text = text;
        pasteField.text = _text;
    }
    
    public string GetText() => _text;
}

  
