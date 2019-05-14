## 備註
該專案 **EnvironmentalAnalysisSystemForBlind_old** 為早期的 EnvironmentalAnalysisSystemForBlind 版本，並且尚未與另一個專案 **ZebraCrossing_Test** 整合。

整合好的專案名叫 **EnvironmentalAnalysisSystemForBlind** 內容也較為豐富

## SURF 筆記
抽取 SURF 特徵的時間約在 100ms - 150ms (解法：可以減少抽取特徵點的畫面來降低時間)
比對的時間在 5ms 左右(BruteForce)
最主要 Delay 的時間在行人偵測 (HOG + SVM) => 會吃掉 500 - 550ms 左右 

#### 加速方式 
1.透過多執行緒來分別處理，先處理好的先等其他還在處理的執行緒，都處理完在發送命令

