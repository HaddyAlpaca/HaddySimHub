namespace iRacingSDK;

public class PitCommand : iRacingMessaging
{
    void SendMessage(PitCommandMode command, int var2 = 0)
    {
        SendMessage(BroadcastMessage.PitCommand, (short)command, var2);
    }

    public void Clear()
    {
        SendMessage(PitCommandMode.Clear);
    }

    public void CleanWindshield()
    {
        SendMessage(PitCommandMode.Windshield);

    }

    public void SetFuel(int amount = 0)
    {
        SendMessage(PitCommandMode.Fuel, amount);
    }

    public void ChangeLeftFrontTire(int kpa = 0)
    {
        SendMessage(PitCommandMode.LeftFront, kpa);
    }

    public void ChangeRightFrontTire(int kpa = 0)
    {
        SendMessage(PitCommandMode.RightFront, kpa);
    }

    public void ChangeLeftRearTire(int kpa = 0)
    {
        SendMessage(PitCommandMode.LeftRear, kpa);
    }

    public void ChangeRightRearTire(int kpa = 0)
    {
        SendMessage(PitCommandMode.RightRear, kpa);
    }

    public void ClearTireChange()
    {
        SendMessage(PitCommandMode.ClearTires);
    }

    public void ChangeAllTyres()
    {
        ChangeLeftFrontTire();
        ChangeRightFrontTire();
        ChangeLeftRearTire();
        ChangeRightRearTire();
    }

    public void RequestFastRepair()
    {
        SendMessage(PitCommandMode.FastRepair);
    }
}
