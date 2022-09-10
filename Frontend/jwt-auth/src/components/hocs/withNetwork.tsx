import React, { FC } from 'react'
import { useAppDispatch, useAppSelector } from '../../hooks/redux'

const withNetwork = (Comp: React.ComponentType<any>) => {
    const WithNetwork: FC = () => {
        const authState = useAppSelector((state) => state.authReducer)
        const dispatch = useAppDispatch()

        if (authState.isLoading) {
            return <div>Загрузка...</div>
        }

        return <Comp authState={authState} dispatch={dispatch} />
    }

    return WithNetwork
}

export default withNetwork
