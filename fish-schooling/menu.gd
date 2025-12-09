extends Control

	
func _on_StartButton_pressed():
	get_tree().change_scene_to_file("res://level.tscn")
	
	#get_tree().change_scene("res://level.tscn");
	
func _on_TutorialButton_pressed():
	get_tree().change_scene_to_file("res://tutorial.tscn")

func _on_QuitButton_pressed():
	get_tree().quit()
