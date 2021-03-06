# 具有特定環境分析之視障輔助系統之設計

## 中文簡介
隨著社會的演進，弱勢族群漸漸受到重視，因此使用科技來解決或協助弱勢族群，使其生活更加友善也成為主要議題。而視障族群雖然有許多可以協助生活的輔具，但是卻仍然有許多困難無法解決，如行走時使用導盲杖無法感測上身的障礙或無法了解周圍環境等問題存在，以致發生危險。 

本論文之貢獻在於提出一套創新的科技輔具概念， 透過穿戴式裝置的軟硬體結合，設計一套可以讓視障者穿戴在身上的輔具系統，透過影像辨識技術，來辨識周遭的環境。本論文著重在視障者行走時，對於周遭環境的商家看板之辨識系統，與偵測路口斑馬線，避免誤闖車道造成危險等系統。

<p align="center">
  <img src="../master/Img-SystemArchitecture.png?raw=true">
</p>

在商家看板系統，透過顏色特徵的彩色直方圖進行畫面上的過濾，再搭配 SURF 特徵做物件辨識；在斑馬線偵測系統，透過霍夫曼直線的尋找、線段的修補與過濾，並分析斑馬線的紋路來判斷。

<p align="center">
  <img src="../master/Img-SignBoardRecognitionSystem.png?raw=true">
</p>

<p align="center">
  <img src="../master/Img-ZebraCrossingDetection.png?raw=true">
</p>


本系統可透過模組化之方式，擴充視障者對此輔具之需求及功能， 開發出對視障者友善與可行之科技輔具。 

## 開發環境
- 程式語言：C#
- IDE : Visual Studio 2012
- 使用的 Library : EmguCV 2.4.0 for x86 (libemgucv-windows-x86-2.4.0.1717.exe)

## Slide 連結
你可以觀看的「具有特定環境分析之視障輔助系統之設計」的 **[Slide 連結](https://www.slideshare.net/secret/jApGLxeH9HebtD)**


## Paper 連結
你可以閱讀「具有特定環境分析之視障輔助系統之設計」的期刊 **[Paper 連結](https://drive.google.com/open?id=1Rb6jZDYyekp01XNCABlrhyIFTKsVg5cm)**
