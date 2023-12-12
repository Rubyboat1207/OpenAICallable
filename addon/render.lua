-- Render function to update the score widget
function render()
  local score = getPersistentString(getCurrentHandle(), "score")

  setTextWidgetContent(getCurrentHandle(), "scoreWidget", "Score: " .. score)
end