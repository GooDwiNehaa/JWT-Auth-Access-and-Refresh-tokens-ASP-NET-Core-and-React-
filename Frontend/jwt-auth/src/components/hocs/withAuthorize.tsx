import React, { FC, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAppDispatch, useAppSelector } from '../../hooks/redux'
import { refresh } from '../../store/action-creators/AuthActionCreators'

const withAuthorize = (Comp: React.ComponentType<any>) => {
    const WithAuthorize: FC = () => {
        const authState = useAppSelector((state) => state.authReducer)
        const dispatch = useAppDispatch()
        const navigate = useNavigate()

        useEffect(() => {
            dispatch(refresh())
        }, [])

        useEffect(() => {
            if (!authState.isAuth) {
                navigate('/login')
            }
        }, [authState.isAuth, navigate])

        return <Comp authState={authState} dispatch={dispatch} navigate={navigate} />
    }
    return WithAuthorize
}

export default withAuthorize
