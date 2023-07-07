开关量输入
[0] = ParaRR3.XIN0;
[1] = ParaRR3.XIN1;
[2] = ParaRR3.XIN2;
[3] = ParaRR3.XIN3;
[4] = ParaRR3.XEXPP;
[5] = ParaRR3.XEXPM;
[6] = ParaRR3.XINPOS;
[7] = ParaRR3.XALARM;
[8] = ParaRR3.YIN0;
[9] = ParaRR3.YIN1;
[10] = ParaRR3.YIN2;
[11] = ParaRR3.YIN3;
[12] = ParaRR3.YEXPP;
[13] = ParaRR3.YEXPM;
[14] = ParaRR3.YINPOS;
[15] = ParaRR3.YALARM;
[16] = ParaRR4.ZIN0;
[17] = ParaRR4.ZIN1;
[18] = ParaRR4.ZIN2;
[19] = ParaRR4.ZIN3;
[20] = ParaRR4.ZEXPP;
[21] = ParaRR4.ZEXPM;
[22] = ParaRR4.ZINPOS;
[23] = ParaRR4.ZALARM;
[24] = ParaRR4.UIN0;
[25] = ParaRR4.UIN1;
[26] = ParaRR4.UIN2;
[27] = ParaRR4.UIN3;
[28] = ParaRR4.UEXPP;
[29] = ParaRR4.UEXPM;
[30] = ParaRR4.UINPOS;
[31] = ParaRR4.UALARM;


[32] = X轴驱动状态  1 正在驱动 0 停止
[33] = Y轴驱动状态  1 正在驱动 0 停止
[34] = Z轴驱动状态  1 正在驱动 0 停止
[35] = U轴驱动状态  1 正在驱动 0 停止


开关量/模拟量输出
[0-31] XYZU 各8个常用输出

## SetDO时, 未用到的通道需要置0

[32-63] StartLVDV命令控制
根据脉冲值(通道32/40/48/56)进行控制是否执行StartLVDV函数, 脉冲值 >0 时将会执行StartLVDV

[32-39] StartLVDV X轴
[32] 脉冲值 0-xxx
[33] DL.Multiple=1;					// 倍率 (1~500) 
[34] DL.Acceleration = 5000;			// 加速度(125~1000,000)(直线加减速驱动中加速度一直不变)
[35] DL.Deceleration = 2500;			// 减速度(125~1000,000)(直线加减速驱动中加速度一直不变)
[36] DL.StartSpeed = 100 ;			// 初始速度(1~8000)
[37] DL.DriveSpeed = 8000;			// 驱动速度	(1~8000)	
[38] 保留通道
[39] 保留通道

[40-47] StartLVDV Y轴
[40] 脉冲值 0-xxx
[41] DL.Multiple=1;					// 倍率 (1~500) 
[42] DL.Acceleration = 5000;			// 加速度(125~1000,000)(直线加减速驱动中加速度一直不变)
[43] DL.Deceleration = 2500;			// 减速度(125~1000,000)(直线加减速驱动中加速度一直不变)
[44] DL.StartSpeed = 100 ;			// 初始速度(1~8000)
[45] DL.DriveSpeed = 8000;			// 驱动速度	(1~8000)	
[46] 保留通道
[47] 保留通道

[48-55] StartLVDV Z轴
[48] 脉冲值 0-xxx
[49] DL.Multiple=1;					// 倍率 (1~500) 
[50] DL.Acceleration = 5000;			// 加速度(125~1000,000)(直线加减速驱动中加速度一直不变)
[51] DL.Deceleration = 2500;			// 减速度(125~1000,000)(直线加减速驱动中加速度一直不变)
[52] DL.StartSpeed = 100 ;			// 初始速度(1~8000)
[53] DL.DriveSpeed = 8000;			// 驱动速度	(1~8000)	
[54] 保留通道
[55] 保留通道

[56-63] StartLVDV U轴
[56] 脉冲值 0-xxx
[57] DL.Multiple=1;					// 倍率 (1~500) 
[58] DL.Acceleration = 5000;			// 加速度(125~1000,000)(直线加减速驱动中加速度一直不变)
[59] DL.Deceleration = 2500;			// 减速度(125~1000,000)(直线加减速驱动中加速度一直不变)
[60] DL.StartSpeed = 100 ;			// 初始速度(1~8000)
[61] DL.DriveSpeed = 8000;			// 驱动速度	(1~8000)	
[62] 保留通道
[63] 保留通道

[64-67] InstStop 命令控制
[64] X 轴
[65] Y 轴
[66] Z 轴
[67] U 轴
