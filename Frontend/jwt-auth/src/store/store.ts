import { configureStore } from '@reduxjs/toolkit'
import { createActionLog } from 'redux-action-log'
import rootReducer from './RootReducer'
import { persistStore, persistReducer, FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER } from 'redux-persist'
import storage from 'redux-persist/lib/storage'

const persistConfig = {
    key: 'root',
    version: 1,
    storage,
    whitelist: ['authReducer'],
}

const persistedReducer = persistReducer<RootState>(persistConfig, rootReducer)

export const actionLog = createActionLog({ limit: 200 })

export const setupStore = () => {
    const store = configureStore({
        reducer: persistedReducer,
        middleware: (getDefaultMiddleware) =>
            getDefaultMiddleware({
                serializableCheck: {
                    ignoredActions: [FLUSH, REHYDRATE, PAUSE, PERSIST, PURGE, REGISTER],
                },
            }),
        enhancers: [actionLog.enhancer],
    })

    const persistor = persistStore(store)

    return {
        store: store,
        persistor: persistor,
    }
}

export type RootState = ReturnType<typeof rootReducer>
export type AppStore = ReturnType<typeof setupStore>
export type AppDispatch = AppStore['store']['dispatch']
