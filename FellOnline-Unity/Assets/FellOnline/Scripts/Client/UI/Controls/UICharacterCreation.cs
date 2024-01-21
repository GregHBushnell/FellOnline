using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FellOnline.Shared;
namespace FellOnline.Client
{
public class UICharacterCreation: FUIControl
{

    public List<SkinnedMeshRenderer> Hair = new List<SkinnedMeshRenderer>();
    public SkinnedMeshRenderer Body;
    public bool startHidden = false;
    private Color color;

     public string hex;
    public int colorint =16777215;

    public FUICharacterCreate fFuicharactercreate;
    

    // Update is called once per frame
    void Update()
    {
    
    }

   


    public void SetColorFirst(Color setcolor)
    {

      color = setcolor;
      hex = color.ToHex();  
      colorint = FHex.ToInt(hex);
       Debug.Log("colorhex: " + hex);
      Debug.Log("colorint: " + colorint);
      Debug.Log("back to color: " + colorint.ToString());
      if(Body != null)
      {
           Body.material.SetColor("_BaseColor",color);
           fFuicharactercreate.SkinColorInt = colorint;
      }else if (Hair.Count > 0)
      {
          foreach (SkinnedMeshRenderer mat in Hair)
          {
              mat.material.SetColor("_BaseColor",color);
              fFuicharactercreate.HairColorInt = colorint;
          }
      }
        
    }

        public override void OnStarting()
        {
            throw new System.NotImplementedException();
        }

        public override void OnDestroying()
        {
            throw new System.NotImplementedException();
        }
    }
}