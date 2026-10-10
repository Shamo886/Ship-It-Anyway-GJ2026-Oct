
using UnityEngine;

public class BTestController : MonoBehaviour
{
    [SerializeField] private ProgrammingManager manager;

    // These methods appear in the component's context menu.

    [ContextMenu("Big - Up")]
    public void BigUp()
    {
        manager.AddBigCommand(GameCommand.Up);
    }

    [ContextMenu("Big - Right")]
    public void BigRight()
    {
        manager.AddBigCommand(GameCommand.Right);
    }

    [ContextMenu("Big - Call Small")]
    public void CallSmall()
    {
        manager.AddBigCommand(GameCommand.CallSmall);
    }

    [ContextMenu("Big - Left")]
    public void BigLeft()
    {
        manager.AddBigCommand(GameCommand.Left);
    }

    [ContextMenu("Big - Down")]
    public void BigDown()
    {
        manager.AddBigCommand(GameCommand.Down);
    }

    [ContextMenu("Small - Up")]
    public void SmallUp()
    {
        manager.AddSmallCommand(GameCommand.Up);
    }

    [ContextMenu("Small - Right")]
    public void SmallRight()
    {
        manager.AddSmallCommand(GameCommand.Right);
    }

    [ContextMenu("Small - Left")]
    public void SmallLeft()
    {
        manager.AddSmallCommand(GameCommand.Left);
    }

    [ContextMenu("Small - Down")]
    public void SmallDown()
    {
        manager.AddSmallCommand(GameCommand.Down);
    }

    [ContextMenu("Undo")]
    public void TestUndo()
    {
        manager.Undo();
    }

    [ContextMenu("Print Packages")]
    public void PrintPackages()
    {
        Debug.Log("BIG: " +
            string.Join(", ", manager.BigPackage));

        Debug.Log("SMALL: " +
            string.Join(", ", manager.SmallPackage));
    }
}
