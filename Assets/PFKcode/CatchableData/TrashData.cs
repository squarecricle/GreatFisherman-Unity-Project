// TrashData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrashData", menuName = "PocketFishingKing/Trash Data")]
public class TrashData : CatchableData
{
    // 目前，垃圾没有特别独特的属性。
    // 它所有的核心数据（名字、图标、行为、权重）都继承自 CatchableData。
    // 未来如果需要，比如想给垃圾添加“可分解出的材料”列表，就可以在这里添加。
}