using Plant01.Upper.Domain.ValueObjects;

namespace Plant01.Upper.Domain.Entities
{
    public class WorkOrder
    {
        /// <summary>
        /// 生产工单号
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 工单日期
        /// </summary>
        public DateOnly OrderDate { get; set; }

        /// <summary>
        /// 产线编号
        /// </summary>
        public string LineNo { get; set; } = string.Empty;

        /// <summary>
        /// 产品编号
        /// </summary>
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// 规格型号
        /// </summary>
        public string ProductSpec { get; set; } = string.Empty;

        /// <summary>
        /// 计划生产数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; } = string.Empty;

        /// <summary>
        /// 批号
        /// </summary>
        public string BatchNumber { get; set; } = string.Empty;

        /// <summary>
        /// 标签模板编号
        /// </summary>
        public string LabelTemplateCode { get; set; } = string.Empty;

        /// <summary>
        /// 工单状态，1开工 99完工
        /// </summary>
        public WorkOrderStatus Status { get; set; }

        /// <summary>
        /// 工单数据
        /// </summary>
        public List<WorkOrderItemProperty> Items { get; set; }

        public override string ToString()
        {
            return $"Code: {Code}, " +
                   $"OrderDate: {OrderDate}, " +
                   $"LineNo: {LineNo}, " +
                   $"ProductCode: {ProductCode}, " +
                   $"ProductName: {ProductName}, " +
                   $"ProductSpec: {ProductSpec}, " +
                   $"Quantity: {Quantity}, " +
                   $"Unit: {Unit}, " +
                   $"BatchNumber: {BatchNumber}, " +
                   $"LabelTemplateCode: {LabelTemplateCode}, " +
                   $"Status: {Status}, " +
                   $"Items: [{string.Join("; ", Items?.Select(i => $"Key: {i.Key}, Name: {i.Name}, Value: {i.Value}") ?? Enumerable.Empty<string>())}]";
        }
    }

    public class WorkOrderItemProperty
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
