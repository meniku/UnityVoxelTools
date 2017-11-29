using UnityEngine;
using System.Collections;
using System;
using NUnit.Framework;

public class NPVoxModelTest
{
    [Test]
    public void Clamp_ShouldReturnClampedVoxCoord()
    {
        NPVoxModel sut = NPVoxModel.NewInstance(new NPVoxCoord(3, 3, 3));
        Assert.AreEqual(new NPVoxCoord(1, 1, 1), sut.Clamp(new NPVoxCoord(1, 1, 1)));
        Assert.AreEqual(new NPVoxCoord(0, 0, 0), sut.Clamp(new NPVoxCoord(-2, -2, -2)));
        Assert.AreEqual(new NPVoxCoord(2, 2, 2), sut.Clamp(new NPVoxCoord(7, 7, 7)));
    }

    [Test]
    public void Clamp_ShouldReturnClampedVoxBox()
    {
        NPVoxModel sut = NPVoxModel.NewInstance(new NPVoxCoord(3, 3, 3));
        NPVoxBox box = sut.Clamp(new NPVoxBox(new NPVoxCoord(-2, -2, -2), new NPVoxCoord(6, 6, 6)));

        Assert.AreEqual(new NPVoxCoord(2, 2, 2), box.RightUpForward);
        Assert.AreEqual(new NPVoxCoord(0, 0, 0), box.LeftDownBack);
    }

}
