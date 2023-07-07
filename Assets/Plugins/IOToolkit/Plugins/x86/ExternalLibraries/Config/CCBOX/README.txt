### 使用说明

// 增加类型点击次数 ETypeID 类型ID _nTypeNumber 第几个按钮(类型的号码)
static void IncTypeClickedNum(int _eTypeID, int _nTypeNumber = 0);

根据前三个通道调用函数
通道0===>非0 表示调用
通道1===>_eTypeID参数
通道2===>_nTypeNumber参数

_eTypeID 值列表
// 按钮 0,
// 参与次数 1,
// 手柄 2,
// 遥感 3,
// 电机启动 4,
// 拉杆 5,
// 旋钮 6,
// 传感器 7,
// 手轮 8,
// 摄像头 9,
// 雷达 10,
// 触摸键 11,
// 按键 12,
// RFID卡 13,
// 转盘 14,
// 滑杆 15,
// 体感器 16,