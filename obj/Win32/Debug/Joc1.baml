   M S B A M L   `   `   `  ����  Nexe Fundació    FaceTrackingBasics.Joc1   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          � Title$  Joc 1��5   	    � WindowState$ 	Maximized=�6    � WindowStyle$
 None=�6/    � 
ResizeMode$ CanMinimize=�6B   .��5      � .+�5   
   � .+�5      �� -   5        $��scrollViewer��5      $��Visible=�6/   $��Disabled=�6W   .��5      � -   5        $��	wrapPanel��5      5	      5
      �� 5   
   -   5         ��Name$ Fondo��5      ��5      �� 5      -   5        grad�� �� 
StartPoint$ 1,2.5&�5   4    �� EndPoint$ 0,0.5&�6G    �� Opacity$	 0.0Q�6X   .��5      � -   5        	Brush�� � Color$ Yellow��5   3   	 � Offset$		 0.0Q�6B   � 5      -   5        
Brush2��$	 Red��5   4   $
	 0.25Q�6@   � 5      -   5        
Brush3��$
 Blue��5   4   $
	 0.75Q�6A   � 5      -	   5        
Brush4��$ 	LimeGreen��5   4   $		 1.0Q�6F   5      .+�5      �� -
   5        $��myRectangle��5      
 �� RadiusX$
 1��6+    �� RadiusY$	 100��67   
����+��$
��5.25��6Y   $	��252��6p   $	��356��6}    �� Left$ 48��6�   
 �� Top$ 48��6�   ��5      �� 5      -   5        	color�� �� Color$ Transparent��5   5    � Triggers 5      :� 5       :� RoutedEvent$ Button.Click��6#   .[�5      �� -   5         ��Name$ MyStoryboard��5   *   .��5      ��  g� AccelerationRatio$	 0.5Q�6)    g� 
SpeedRatio$ 15Q�6A   .��5   "   W�  �� 
TargetName$ myRectangle��62    �� TargetProperty$ RadiusX�6V    g� Duration$ 0:0:4D�6z    W� From$	 100X�6�   	 W� To$ 10X�6�    g� AutoReverse .  g� FillBehavior$
 Stop=�6�   W� 5   "   $ myRectangle��62   $ RadiusY�6V   $ 0:0:4D�6z   $	 100X�6�   $ 10X�6�    . $
 Stop=�6�   W� 5    "   $ myRectangle��62   $ Height�6V   $ 0:0:8D�6y   $ 10X�6�   $	 207X�6�    . $
 Stop=�6�   W� 5!   "   $ myRectangle��62   $ Width�6V   $ 0:0:8D�6x   $ 10X�6�   $	 267X�6�    . $
 Stop=�6�   �� 5#   "   $ color��61   $ Color�6O   $ 0:0:8D�6q    �� From$ TransparentX�6�   	 �� To$
 BlueX�6�    . $
 Stop=�6�   W� 5$   "   $
 grad��62   $ Opacity�6O   $ 0:0:30D�6s   $	 0.0X�6�   $	 1.0X�6�   $
 Stop=�6�   �� 5%   "   $ Brush��61   $ Color�6O   $ 0:0:1D�6q   $ YellowX�6�   $	 RedX�6�    .  g� RepeatBehavior$ 0:0:30��6�   �� 5&   "   $ Brush2��61   $ Color�6P   $ 0:0:1D�6r   $ BlackX�6�   $
 BlueX�6�    . $ 0:0:30��6�   $
 Stop=�6�   �� 5'   "   $ Brush3��61   $ Color�6P   $ 0:0:1D�6r   $ GreenX�6�   $	 RedX�6�    . $ 0:0:30��6�   �� 5(   "   $ Brush4��61   $ Color�6P   $ 0:0:1D�6r   $
 BlueX�6�   $ YellowX�6�    . $ 0:0:30��6�   $
 Stop=�6�   5)      5*      5+      5-      5/      �� 50   
   -   5        $��Sortir��50      $��Sortir��6   $
��Left=�60   $��
660,31,0,0q�6K   $	��Top=�6_   $��75��6w   6�   51      52      