namespace Domain.Enums
{
    public enum WalletStatus
    {
        Prcs, // Ожидает открытия
        Actv, // Активен
        Blck,  // Заблокирован
        Clsd, // Закрыт (финальный)
    }
}
