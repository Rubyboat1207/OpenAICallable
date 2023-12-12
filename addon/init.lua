-- Set the name of the HUD window
setHudWindowName(getCurrentHandle(), "MyExtension")

-- Add a text widget for the score
addTextWidget(getCurrentHandle(), "scoreText", 0)

-- Set the initial score value
setPersistentFloat(getCurrentHandle(), "score", 0)

-- Add a button widget
addButtonWidget(getCurrentHandle(), "addButton", 1)

-- Set the button label
setButtonWidgetLabel(getCurrentHandle(), "addButton", "Add One")

-- Set the button callback
setButtonOnClick(getCurrentHandle(), "addButton", "addOne")

-- Function to handle the button click
function addOne()
  -- Get the current score
  local score = getPersistentFloat(getCurrentHandle(), "score")
  
  -- Increment the score
  score = score + 1
  
  -- Update the score text widget
  setTextWidgetContent(getCurrentHandle(), "scoreText", "Score: " .. score)
  
  -- Update the persistent score value
  setPersistentFloat(getCurrentHandle(), "score", score)
end