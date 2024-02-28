using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
public class PlayerIK : MonoBehaviour
{
    private Transform LeftHandIKTarget;
    private Transform RightHandIKTarget;
    private Transform LeftElbowIKTarget;
    private Transform RightElbowIKTarget;

    [SerializeField] private TwoBoneIKConstraint LeftHandIK;
    [SerializeField] private TwoBoneIKConstraint RightHandIK;

    [SerializeField] private Rig HandIK;

    [SerializeField] private RigBuilder rigBuilder;

    public void Setup(Transform GunParent)
    {
        Transform[] allChildren = GunParent.GetComponentsInChildren<Transform>();

        RightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");
        RightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
        LeftElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbow");
        LeftHandIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftHand");

        RightHandIK.data.target = RightHandIKTarget;
        RightHandIK.data.hint = RightElbowIKTarget;
        LeftHandIK.data.target = LeftHandIKTarget;
        LeftHandIK.data.hint = LeftElbowIKTarget;

        HandIK.weight = 1f;
        rigBuilder.Build();
    }
}