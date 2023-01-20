module Page.SignUp exposing (
  Model,
  Msg,
  init,
  session,
  subscriptions,
  update,
  view )

import Browser exposing (Document)
import Browser.Navigation as Nav
import Html exposing (Html)
import Html.Attributes as Attr
import Html.Lazy as Lazy
import Html.Events as Events
import Http

import Session exposing (..)
import Route exposing (Route, SignUpFragment(..))
import Helpers exposing (..)
import Port
import Api


type alias Model = 
  { session : Session
  , section : Section
  }


type Section
  = SignUpForm FormModel
  | Success SuccessModel


type alias FormModel =
  { username : String
  , email : String
  , captchaFilled : Bool
  , captchaToken : String
  , errorMessage : String
  }


type alias SuccessModel =
  { email : Maybe String
  }


type Msg
  = Username String
  | Email String
  | CaptchaFilled String
  | CaptchaExpired
  | CaptchaError
  | SignUpRequest
  | SignUpResponse (Api.Result ())


init : Session -> SignUpFragment -> ( Model, Cmd Msg )
init oldSession fragment =
  ( { session = oldSession
    , section = (
        case fragment of
          JustSignUp ->
            SignUpForm
              { email = ""
              , username = ""
              , captchaFilled = False
              , captchaToken = ""
              , errorMessage = ""
              }

          SignUpSuccess ->
            Success
              { email = Nothing
              }
      )
    }
  , if fragment == JustSignUp then
      Port.reloadCaptcha ()
    else
      Cmd.none
  )


session : Model -> Session
session model = model.session


subscriptions : Model -> Sub Msg
subscriptions _ =
  Sub.batch
    [ Port.captchaFilled CaptchaFilled
    , Port.captchaExpired (Port.notify CaptchaExpired)
    , Port.captchaError (Port.notify CaptchaError)
    ]


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
  case ( msg, model.section ) of
    ( Username username, SignUpForm form ) ->
      ( { model | section = SignUpForm { form | username = username } }
      , Cmd.none
      )

    ( Email email, SignUpForm form ) ->
      ( { model | section = SignUpForm { form | email = email } }
      , Cmd.none
      )

    ( CaptchaFilled token, SignUpForm form ) ->
      ( { model | section = SignUpForm
          { form | captchaFilled = True , captchaToken = token }
        }
      , Cmd.none
      )

    ( CaptchaExpired, SignUpForm form ) ->
      ( { model | section = SignUpForm
          { form | captchaFilled = False , captchaToken = "" }
        }
      , Cmd.none
      )

    ( CaptchaError, SignUpForm form ) ->
      ( { model | section = SignUpForm
          { form | captchaFilled = False , captchaToken = "" }
        }
      , Cmd.none
      )

    ( SignUpRequest, SignUpForm form ) ->
      if form.captchaFilled then
        ( model
        , Api.signUpRequest SignUpResponse
          { captcha = form.captchaToken
          , email = form.email
          , username = form.username }
        )
      else
        ( { model | section = SignUpForm
            { form | errorMessage =
              "Le CAPTCHA n'a pas été validé ou a expiré"
            }
          }
        , Cmd.none
        )

    ( SignUpResponse response, SignUpForm form ) ->
      case response of
        Err Api.Internal ->
          ( { model | section = SignUpForm
              { form | errorMessage = "Erreur interne" }
            }
          , Cmd.none
          )
        Err Api.Network ->
          ( { model | section = SignUpForm
              { form | errorMessage = "Impossible de se connecter au serveur" }
            }
          , Cmd.none
          )
        Err Api.Unauthorized ->
          ( { model | section = SignUpForm
              { form | errorMessage = "Erreur interne" }
            }
          , Cmd.none
          )
        Err (Api.Message message) ->
          ( { model | section = SignUpForm
              { form | errorMessage = message }
            }
          , Cmd.none
          )
        Ok () ->
          ( { model | section = Success
              { email = Just form.email }
            }
          , Nav.replaceUrl model.session.key "/myaccount" )

    _ ->
      ( model, Cmd.none )


view : Model -> Html Msg
view model =
  Html.main_ [ ]
    [ Html.div [ Attr.class "box" ]
      ( Html.h1 [ ] [ Html.text "Inscription" ]
      :: ( case model.section of
            SignUpForm form ->
              viewForm form

            Success success ->
              viewSuccess success
        )
      )
    ]


viewForm : FormModel -> List (Html Msg)
viewForm form =
  [ Html.p
    [ Attr.class (
        if String.isEmpty form.errorMessage then "no-error" else "error"
      )
    ]
    [ Html.text form.errorMessage
    ]
  , Html.form [ Attr.id "signup", Events.onSubmit SignUpRequest ]
    [ Lazy.lazy formInput
        { id = "username"
        , type_ = "text"
        , label = "Nom d'utilisateur"
        , required = True
        , onInput = Username
        }
    , Lazy.lazy formInput
        { id = "email"
        , type_ = "email"
        , label = "Addresse mail"
        , required = True
        , onInput = Email
        }
    , Html.div [ Attr.class "captcha" ]
      [ Html.div [ Attr.id "recaptcha" ] [ ]
      ]
    , Html.div [ Attr.class "input" ]
      [ Html.input [ Attr.type_ "submit", Attr.value "S'inscrire" ] [ ]
      ]
    ]
  ]

viewSuccess : SuccessModel -> List (Html Msg)
viewSuccess model =
  [ Html.p [ ] [ Html.text "Inscription réussie !" ]
  , Html.p [ ]
    [ Html.text (
        "Un e-mail de confirmation vous a été envoyé"
        ++ (
          Maybe.withDefault ""
            (Maybe.map (\email -> " à l'adresse " ++ email) model.email)
        )
        ++ "."
      )
    ]
  , Html.p [ ]
    [ Html.i [ ]
      [ Html.text (
          "Si vous ne recevez pas d'e-mail, pensez à regarder "
          ++ "votre boîte de spam. Vous pouvez aussi essayer de vous "
          ++ "réinscrire avec le même nom d'utilisateur."
        )
      ]
    ]
  ]