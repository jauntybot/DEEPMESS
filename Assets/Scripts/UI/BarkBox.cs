using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarkBox : MonoBehaviour {

    AudioSource audioSource;
    [SerializeField] SFX barkSFX;

    public enum BarkType { Delay, FinalFloor, EnemyCount, NotPrimed, LowHP, SlagHurt, NailKill, NailCrush, NailPrime };
    [SerializeField] GameObject barkBox;
    [SerializeField] TMP_Text barkBoxText;
    
    [SerializeField] List<string> delayStrings, finalFloorStrings, EnemyCountStrings, NotPrimedStrings, LowHPStrings, SlagHurtStrings, NailKillStrings, NailCrushStrings, NailPrimeStrings;
    
    Nail nail;

    public void Init(Nail n) {
        audioSource = GetComponent<AudioSource>();
        nail = n;
    }

    public void Bark(BarkType type) {
        if (FloorManager.instance.floorSequence.activePacket.packetType != FloorChunk.PacketType.Tutorial
            && nail.nailState != Nail.NailState.Falling) {
            barkBox.SetActive(false);
            string bark;
            switch (type) {
                case BarkType.Delay: 
                    bark = delayStrings[Random.Range(0, delayStrings.Count - 1)];
                break;
                case BarkType.FinalFloor: 
                    bark = finalFloorStrings[Random.Range(0, finalFloorStrings.Count - 1)];
                break;
                case BarkType.EnemyCount: 
                    bark = EnemyCountStrings[Random.Range(0, EnemyCountStrings.Count - 1)];
                break;
                case BarkType.NotPrimed: 
                    bark = NotPrimedStrings[Random.Range(0, NotPrimedStrings.Count - 1)];
                break;
                case BarkType.LowHP: 
                    bark = LowHPStrings[Random.Range(0, LowHPStrings.Count - 1)];
                break;
                default:
                case BarkType.SlagHurt: 
                    bark = SlagHurtStrings[Random.Range(0, SlagHurtStrings.Count - 1)];
                break;
                case BarkType.NailKill: 
                    bark = NailKillStrings[Random.Range(0, NailKillStrings.Count - 1)];
                break;
                case BarkType.NailCrush: 
                    bark = NailCrushStrings[Random.Range(0, NailCrushStrings.Count - 1)];
                break;
                case BarkType.NailPrime: 
                    bark = NailPrimeStrings[Random.Range(0, NailPrimeStrings.Count - 1)];
                break;
            }

            // if (bark.ToCharArray().Length > 20)
            //     barkBox.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            // else    
            //     barkBox.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.MinSize;
            
            barkBoxText.text = bark;
            barkBox.SetActive(true);
            
            if (barkSFX) {
                if (barkSFX.outputMixerGroup)
                    audioSource.outputAudioMixerGroup = barkSFX.outputMixerGroup;

                audioSource.PlayOneShot(barkSFX.Get());
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(barkBox.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();
        }
    }


}
