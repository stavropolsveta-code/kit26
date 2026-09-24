namespace EchoOfAncients.Core
{
    /// <summary>
    /// Пассивные правила поля, которые здания города включают на игровом поле.
    /// Хранится в Core, чтобы резолвер не зависел от Meta.
    /// </summary>
    public class FieldRules
    {
        /// <summary>Кузница: T-образные матчи оставляют Уголь (пробивает 2 слоя льда).</summary>
        public bool ForgeEmber;

        /// <summary>Древо Жизни: L-образные матчи оставляют Семена (прорастают в новые фишки).</summary>
        public bool TreeSeeds;

        public static FieldRules None() => new FieldRules();
    }
}