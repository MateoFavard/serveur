module Main exposing (..)

import Browser
import Browser.Navigation as Nav
import Html exposing (Html)
import Html.Attributes as Attr
import Html.Events as Events
import Url

import Page.Home
import Page.SignUp
import Page.SignIn
import Page.Validation
import Page.MyAccount
import Page.NotFound

import Route exposing (Route)
import Session exposing (..)


main : Program () Model Msg
main =
  Browser.application
    { init = init
    , update = update
    , subscriptions = subscriptions
    , view = view
    , onUrlChange = UrlChanged
    , onUrlRequest = UrlRequest
    }


type Model
  = Home Page.Home.Model
  | SignUp Page.SignUp.Model
  | SignIn Page.SignIn.Model
  | Validation Page.Validation.Model
  | MyAccount Page.MyAccount.Model
  | NotFound Page.NotFound.Model


init : () -> Url.Url -> Nav.Key -> ( Model, Cmd Msg )
init flags url key =
  changeRoute
    { key = key
    , route = Route.NotFound
    , user = Visitor
    }
    (Route.fromUrl url)


type Msg
  = UrlRequest Browser.UrlRequest
  | UrlChanged Url.Url
  | SignOut
  | MsgHome Page.Home.Msg
  | MsgSignUp Page.SignUp.Msg
  | MsgSignIn Page.SignIn.Msg
  | MsgValidation Page.Validation.Msg
  | MsgMyAccount Page.MyAccount.Msg
  | MsgNotFound Page.NotFound.Msg


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
  case ( msg, model ) of
    ( UrlRequest urlRequest, _ ) ->
      case urlRequest of
        Browser.Internal url ->
          ( model, Nav.pushUrl (getSession model).key (Url.toString url) )

        Browser.External href ->
          ( model, Nav.load href )

    ( UrlChanged url, _ ) ->
      changeRoute (getSession model) (Route.fromUrl url)

    ( SignOut, _ ) ->
      signOut (getSession model)

    ( MsgHome msgHome, Home home ) ->
      Page.Home.update msgHome home
        |> toUpdate Home MsgHome

    ( MsgSignUp msgSignUp, SignUp signUp ) ->
      Page.SignUp.update msgSignUp signUp
        |> toUpdate SignUp MsgSignUp

    ( MsgSignIn msgSignIn, SignIn signIn ) ->
      Page.SignIn.update msgSignIn signIn
        |> toUpdate SignIn MsgSignIn

    ( MsgValidation msgValidation, Validation validation ) ->
      Page.Validation.update msgValidation validation
        |> toUpdate Validation MsgValidation

    ( MsgMyAccount msgMyAccount, MyAccount myAccount ) ->
      Page.MyAccount.update msgMyAccount myAccount
        |> toUpdate MyAccount MsgMyAccount

    ( MsgNotFound msgNotFound, NotFound notFound ) ->
      Page.NotFound.update msgNotFound notFound
        |> toUpdate NotFound MsgNotFound

    _ ->
      ( model, Cmd.none )


toUpdate : 
  (model -> Model)
  -> (msg -> Msg)
  -> ( model, Cmd msg )
  -> ( Model, Cmd Msg )
toUpdate toModel toMsg ( model, cmd ) =
  ( toModel model, Cmd.map toMsg cmd )


changeRoute : Session -> Route -> ( Model, Cmd Msg )
changeRoute session newRoute =
  let
    newSession = { session | route = newRoute }
  in
    case newRoute of
      Route.Home ->
        Page.Home.init newSession
          |> toUpdate Home MsgHome

      Route.SignUp signUpFragment ->
        Page.SignUp.init newSession signUpFragment
          |> toUpdate SignUp MsgSignUp

      Route.SignIn ->
        Page.SignIn.init newSession
          |> toUpdate SignIn MsgSignIn

      Route.Validation validationFragment->
        Page.Validation.init newSession validationFragment
          |> toUpdate Validation MsgValidation

      Route.MyAccount myAccountFragment ->
        Page.MyAccount.init newSession myAccountFragment
          |> toUpdate MyAccount MsgMyAccount

      _ ->
        Page.NotFound.init newSession
          |> toUpdate NotFound MsgNotFound


signOut : Session -> ( Model, Cmd Msg )
signOut session =
    Page.Home.init (visitor session)
      |> toUpdate Home MsgHome


getSession : Model -> Session
getSession model =
  case model of
    Home home ->
      Page.Home.session home

    SignUp signUp ->
      Page.SignUp.session signUp

    SignIn signIn ->
      Page.SignIn.session signIn

    MyAccount myAccount ->
      Page.MyAccount.session myAccount

    Validation validation ->
      Page.Validation.session validation

    NotFound notFound ->
      Page.NotFound.session notFound


subscriptions : Model -> Sub Msg
subscriptions model =
  case model of
    SignUp signUp ->
      Sub.map MsgSignUp (Page.SignUp.subscriptions signUp)

    _ ->
      Sub.none


view : Model -> Browser.Document Msg
view model =
  case model of
    Home home ->
      Page.Home.view home
        |> toView (getSession model) MsgHome "LOTD"

    SignUp signUp ->
      Page.SignUp.view signUp
        |> toView (getSession model) MsgSignUp "Inscription — LOTD"

    SignIn signIn ->
      Page.SignIn.view signIn
        |> toView (getSession model) MsgSignIn "Connexion — LOTD"

    Validation validation ->
      Page.Validation.view validation
        |> toView (getSession model) MsgValidation "Validation du compte — LOTD"

    MyAccount myAccount ->
      Page.MyAccount.view myAccount
        |> toView (getSession model) MsgMyAccount "Mon compte — LOTD"

    NotFound notFound ->
      Page.NotFound.view notFound
        |> toView (getSession model) MsgNotFound "Erreur 404 — LOTD"


toView :
  Session
  -> (msg -> Msg)
  -> String
  -> Html msg
  -> Browser.Document Msg
toView session toMsg title body =
  { title = title
  , body = 
    [ Html.header [ ]
      [ Html.a [ Attr.href "/" ]
        [ Html.img [ Attr.src "/static/image.png", Attr.alt "Lord Of The Dungeons" ] [ ]
        ]
      , viewHeader session 
      ]
    , Html.map toMsg body
    , Html.footer [ ]
      [ Html.text "© 2022 IUT Games / Fraude Studio"
      ]
    ]
  }


viewHeader : Session -> Html Msg
viewHeader session =
  case session.user of 
    Validated _ ->
      Html.div [ ]
        [ case session.route of
            Route.MyAccount _ ->
              Html.span [ ] [ ]
            _ ->
              Html.a [ Attr.href "/myaccount" ] [ Html.text "Mon compte" ]
        , Html.button [ Events.onClick SignOut ] [ Html.text "Déconnexion" ]
        ]

    _ ->
      Html.div [ ]
        [ Html.a [ Attr.href "/signup" ] [ Html.text "Inscription" ]
        , Html.a [ Attr.href "/signin" ] [ Html.text "Connexion" ]
        ]