# Tag 值转换表达式说明

## 目标
在 `DeviceCommunication` 标签配置中，为每个 Tag 增加可选的“源值 -> 目标值”转换表达式能力。

## 配置字段
在标签 CSV 中新增可选列（支持任一列名）：
- `ValueTransformExpression`（推荐）
- `TransformExpression`
- `Expression`

未配置时，系统保持原始值不变。

## 变量与写法
表达式变量支持：`value` / `x` / `source`（三者等价）。

示例：
- `value * 0.1`
- `x + 100`
- `source / 60`
- `=1`（等价于 `value = 1`，可用于触发位转布尔）

## 生效时机
在驱动读取到原始值后、写入 `CommunicationTag` 快照前执行转换。

## 异常处理
- 表达式执行失败时，自动回退为原始值。
- 同一 Tag + 表达式组合仅记录一次告警日志，避免刷屏。

## 注意事项
- 表达式结果会按标签 `DataType` 自动转换。
- 当 `ArrayLength > 1` 且非字符串时，默认不对数组逐项转换（保持原值）。
