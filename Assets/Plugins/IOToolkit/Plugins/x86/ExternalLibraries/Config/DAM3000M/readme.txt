		
输出通道:
0-15 开关量输出
16-18 模拟量输出  16 电流量程(0: 0～20mA; 1: 4～20mA)		17 电流输出值		18 电压输出值(0.0 ～ 10.0V)

模拟量输入:
		AD_lMode
		printf("请输入AD模式\n");
		printf(" 0: 0～10mA \n");
		printf(" 1:  -15～+15mV\n");
		printf(" 2: -50～+50mV\n");
		printf(" 3: -100～+100mV\n");
		printf(" 4: -150～+150m\n");
		printf(" 5: -500～+500mV\n");
		printf(" 6: -1～+1V\n");
		printf(" 7: -2.5～+2.5V\n");
		printf(" 8: -5～+5V\n");
		printf(" 9: -10～+10V\n");
		printf(" D: 0～+5V\n");
		printf(" E: 0～+10V\n");
		printf(" F:  0～+2.5V\n");
		printf(" A: -20～+20mA\n");
		printf(" B: 0～20mA\n");
		printf(" C: 4～20mA\n");
		printf(" 80: 0～22mA\n");
		printf(" 10: J型热电偶   0～1200℃\n");
		printf(" 11: K型热电偶   0～1300℃\n");
		printf(" 12: T型热电偶-200～400℃\n");
		printf(" 13: E型热电偶   0～1000℃\n");
		printf(" 14: R型热电偶 500～1700℃\n");
		printf(" 15: S型热电偶 500～1768℃\n");
		printf(" 16: B型热电偶 500～1800℃\n");
		printf(" 17: N型热电偶   0～1300℃\n");
		printf(" 18: C型热电偶   0～2090℃\n");
		printf(" 19: 钨铼5-钨铼26 0～2310℃\n");
		printf(" 20: Pt100(385)热电阻 -200℃～850℃\n");
		printf(" 21: Pt100(385)热电阻 -100℃～100℃\n");
		printf(" 22: Pt100(385)热电阻    0℃～100℃\n");
		printf(" 23: Pt100(385)热电阻    0℃～200℃\n");
		printf("24: Pt100(385)热电阻    0℃～600℃\n");
		printf(" 25: Pt100(3916)热电阻-200℃～850℃\n");
		printf(" 26: Pt100(3916)热电阻-100℃～100℃\n");
		printf(" 27: Pt100(3916)热电阻   0℃～100℃\n");
		printf(" 28: Pt100(3916)热电阻   0℃～200℃\n");
		printf(" 29: Pt100(3916)热电阻   0℃～600℃\n");
		printf(" 30:  Pt1000热电阻     -200℃～850℃\n");
		printf(" 40: Cu50热电阻        -50℃～150℃\n");
		printf(" 41: Cu100热电阻       -50℃～150℃\n");
		printf(" 42: BA1热电阻        -200℃～650℃\n");
		printf(" 43: BA2热电阻        -200℃～650℃\n");
		printf(" 44: G53热电阻         -50℃～150℃\n");
		printf(" 45: Ni50热电阻        100℃\n");
		printf(" 46: Ni508热电阻         0℃～100℃\n");
		printf(" 47: Ni1000热电阻      -60℃～160℃\n");