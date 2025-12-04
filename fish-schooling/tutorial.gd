extends Control

func _ready():
	pass
	
func _on_Back_pressed():
	get_tree().change_scene_to_file("res://menu.tscn")
	#get_tree().change_scene("res://level.tscn");
