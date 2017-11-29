using UnityEngine;
using System.Collections;
using System;
using NUnit.Framework;

public class NPVoxBoxTest
{
    [Test]
    public void Size_ShouldReturnCorrectSize()
    {
        NPVoxBox sut = new NPVoxBox(new NPVoxCoord(1,1,1), new NPVoxCoord(4,4,4));
        Assert.AreEqual(4, sut.Size.X);
        Assert.AreEqual(4, sut.Size.Y);
        Assert.AreEqual(4, sut.Size.Z);
    }
    
    [Test]
    public void Center_ShouldReturnCorrectCenter()
    {
        NPVoxBox sut = new NPVoxBox(new NPVoxCoord(1,1,1), new NPVoxCoord(3,3,3));
        Assert.AreEqual(2, sut.Center.X);
        Assert.AreEqual(2, sut.Center.Y);
        Assert.AreEqual(2, sut.Center.Z);
    }
    
    [Test]
    public void FromCenterSize_ShouldConstructCorrectBox()
    {
        NPVoxBox sut = NPVoxBox.FromCenterSize(new NPVoxCoord(2,2,2), new NPVoxCoord(3,3,3));
        Assert.AreEqual(2, sut.Center.X);
        Assert.AreEqual(2, sut.Center.Y);
        Assert.AreEqual(2, sut.Center.Z);
        Assert.AreEqual(3, sut.Size.X);
        Assert.AreEqual(3, sut.Size.Y);
        Assert.AreEqual(3, sut.Size.Z);
        Assert.AreEqual(1, sut.LeftDownBack.X);
        Assert.AreEqual(1, sut.LeftDownBack.Y);
        Assert.AreEqual(1, sut.LeftDownBack.Z);
        Assert.AreEqual(3, sut.RightUpForward.X);
        Assert.AreEqual(3, sut.RightUpForward.Y);
        Assert.AreEqual(3, sut.RightUpForward.Z);
    }

    [Test]
    public void Contains_ShouldReturnTrueWhenInBox()
    {
        NPVoxBox sut = NPVoxBox.FromCenterSize(new NPVoxCoord(2,2,2), new NPVoxCoord(3,3,3));
        Assert.IsTrue(sut.Contains(new NPVoxCoord(2,2,2)));
        Assert.IsTrue(sut.Contains(new NPVoxCoord(1,1,1)));
        Assert.IsTrue(sut.Contains(new NPVoxCoord(3,3,3)));
    }
    
    [Test]
    public void Contains_ShouldReturnFalseWhenInBox()
    {
        NPVoxBox sut = NPVoxBox.FromCenterSize(new NPVoxCoord(2,2,2), new NPVoxCoord(3,3,3));
        Assert.IsFalse(sut.Contains(new NPVoxCoord(0,0,0)));
        Assert.IsFalse(sut.Contains(new NPVoxCoord(4,4,4)));
    }
}
