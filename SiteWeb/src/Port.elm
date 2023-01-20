port module Port exposing (..)

port captchaFilled : (String -> msg) -> Sub msg

port captchaExpired : (() -> msg) -> Sub msg

port captchaError : (() -> msg) -> Sub msg

port reloadCaptcha : () -> Cmd msg


notify : msg -> () -> msg
notify msg _ = msg